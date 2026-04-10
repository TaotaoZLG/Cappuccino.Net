using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Cappuccino.Common.Enum;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Log;
using Cappuccino.Common.Util;

namespace Cappuccino.Common.Helper
{
    public class FileHelper
    {
        #region 上传下载操作
        #region 上传单个文件
        /// <summary>
        /// 上传单个文件
        /// </summary>
        /// <param name="fileModule">上传模块标识</param>
        /// <param name="file">待上传的文件通过HttpPostedFileBase接收</param>
        /// <returns></returns>
        public async static Task<TData<string>> UploadFile(int fileModule, HttpPostedFileBase file)
        {
            string dirModule = string.Empty;
            TData<string> obj = new TData<string>();

            // 1. 校验文件是否存在
            if (file == null || file.ContentLength <= 0)
            {
                obj.Message = "请先选择文件！";
                return obj;
            }

            // 2. 按模块校验文件类型/大小
            TData objCheck = null;
            string fileExtension = Path.GetExtension(file.FileName);
            switch (fileModule)
            {
                case (int)UploadFileType.Portrait:
                    objCheck = CheckFileExtension(fileExtension, ".jpg|.jpeg|.gif|.png");
                    if (objCheck.Status != 1)
                    {
                        obj.Message = objCheck.Message;
                        return obj;
                    }
                    dirModule = UploadFileType.Portrait.ToString();
                    break;

                case (int)UploadFileType.News:
                    // 校验文件大小（5MB）
                    if (file.ContentLength > 5 * 1024 * 1024)
                    {
                        obj.Message = "文件最大限制为 5MB";
                        return obj;
                    }
                    objCheck = CheckFileExtension(fileExtension, ".jpg|.jpeg|.gif|.png");
                    if (objCheck.Status != 1)
                    {
                        obj.Message = objCheck.Message;
                        return obj;
                    }
                    dirModule = UploadFileType.News.ToString();
                    break;

                case (int)UploadFileType.Import:
                    objCheck = CheckFileExtension(fileExtension, ".xls|.xlsx|.doc|.docx");
                    if (objCheck.Status != 1)
                    {
                        obj.Message = objCheck.Message;
                        return obj;
                    }
                    dirModule = UploadFileType.Import.ToString();
                    break;

                default:
                    obj.Message = "请指定上传到的模块";
                    return obj;
            }

            // 3. 生成新文件名和存储目录
            // 兜底文件扩展名（默认.png）
            string finalFileExtension = TextHelper.GetCustomValue(fileExtension, ".png");
            // 生成唯一文件名（GUID）
            string newFileName = GuidHelper.GetGuid(true) + finalFileExtension;
            // 构建相对目录（按模块+日期分目录）
            string relativeDir = $"Resource{Path.DirectorySeparatorChar}{dirModule}{Path.DirectorySeparatorChar}" +
                                 $"{DateTime.Now.ToString("yyyy-MM-dd").Replace('-', Path.DirectorySeparatorChar)}{Path.DirectorySeparatorChar}";

            // 4. 构建绝对路径
            string absoluteDir = HostingEnvironment.MapPath("~/" + relativeDir.Replace(Path.DirectorySeparatorChar, '/'));
            string absoluteFileName = Path.Combine(absoluteDir, newFileName);

            // 5. 确保目录存在
            if (!Directory.Exists(absoluteDir))
            {
                Directory.CreateDirectory(absoluteDir);
            }

            // 6. 保存文件（异步方式）
            try
            {
                using (FileStream fs = File.Create(absoluteFileName))
                {
                    // 从HttpPostedFileBase的InputStream异步复制到文件流
                    await file.InputStream.CopyToAsync(fs);
                    fs.Flush();
                }

                // 7. 组装返回结果
                // 转换目录为HTTP可访问格式（替换分隔符）
                string httpDir = ConvertDirectoryToHttp(relativeDir);
                obj.Data = $"{Path.AltDirectorySeparatorChar}{httpDir}{newFileName}";
                // 原文件名（无扩展名）
                obj.Message = Path.GetFileNameWithoutExtension(TextHelper.GetCustomValue(file.FileName, newFileName));
                // 文件大小（KB）
                obj.Description = (file.ContentLength / 1024).ToString();
                obj.Status = 1; // 上传成功标识
            }
            catch (Exception ex)
            {
                obj.Message = ex.Message;
            }

            return obj;
        }
        #endregion

        #region 批量上传文件
        /// <summary>
        /// 批量上传文件
        /// </summary>
        /// <param name="fileModule"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public async static Task<TData<string>> UploadFile(int fileModule, IEnumerable<HttpPostedFileBase> files)
        {
            if (files == null || !files.Any())
            {
                TData<string> obj = new TData<string>();
                obj.Message = "请先选择文件！";
                return obj;
            }
            return await UploadFile(fileModule, files);
        }
        #endregion

        #region 删除单个文件
        /// <summary>
        /// 删除单个文件
        /// </summary>
        /// <param name="fileModule"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static TData<string> DeleteFile(int fileModule, string filePath)
        {
            TData<string> obj = new TData<string>();
            string dirModule = fileModule.GetDescriptionByEnum<UploadFileType>();

            if (string.IsNullOrEmpty(filePath))
            {
                obj.Message = "请先选择文件！";
                return obj;
            }

            filePath = FilterFilePath(filePath);
            //filePath = "Resource" + Path.DirectorySeparatorChar + dirModule + Path.DirectorySeparatorChar + filePath;
            string absoluteDir = Path.Combine(HostingEnvironment.MapPath(filePath));
            try
            {
                if (File.Exists(absoluteDir))
                {
                    File.Delete(absoluteDir);
                }
                else
                {
                    obj.Message = "文件不存在";
                }
                obj.Status = 1;
            }
            catch (Exception ex)
            {
                obj.Message = ex.Message;
            }
            return obj;
        }
        #endregion

        #region 下载文件
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="delete"></param>
        /// <returns></returns>
        public static TData<FileContentResult> DownloadFile(string filePath, int delete)
        {
            filePath = FilterFilePath(filePath);
            if (!filePath.StartsWith("Resource", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("非法访问");
            }
            TData<FileContentResult> obj = new TData<FileContentResult>();
            string rootPath = HttpContext.Current != null ? HostingEnvironment.MapPath("~") : AppDomain.CurrentDomain.BaseDirectory;
            // 安全拼接路径（避免重复分隔符）
            string absoluteFilePath = Path.Combine(rootPath, filePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
            byte[] fileBytes = File.ReadAllBytes(absoluteFilePath);
            if (delete == 1)
            {
                File.Delete(absoluteFilePath);
            }
            // md5 值
            string fileNamePrefix = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string title = fileNameWithoutExtension;
            if (fileNameWithoutExtension.Contains("_"))
            {
                title = fileNameWithoutExtension.Split('_')[1].Trim();
            }
            string fileExtensionName = Path.GetExtension(filePath);
            obj.Data = new FileContentResult(fileBytes, "application/octet-stream")
            {
                FileDownloadName = string.Format("{0}_{1}{2}", fileNamePrefix, title, fileExtensionName)
            };
            obj.Status = 1;
            return obj;
        }
        #endregion
        #endregion

        #region 路径转换操作
        /// <summary>
        /// 获取物理路径（转换虚拟路径）
        /// </summary>
        public static string GetPhysicalPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }

        /// <summary>
        /// 将物理路径转换为虚拟路径
        /// </summary>
        /// <param name="physicalPath">物理路径</param>
        /// <returns>虚拟路径（以"~/"开头）</returns>
        public static string GetVirtualPath(string physicalPath)
        {
            try
            {
                // 获取应用程序物理路径
                string appPhysicalPath = HttpContext.Current != null ? HostingEnvironment.MapPath("~/") : AppDomain.CurrentDomain.BaseDirectory;

                // 确保路径是绝对路径
                physicalPath = Path.GetFullPath(physicalPath);
                appPhysicalPath = Path.GetFullPath(appPhysicalPath);

                // 验证物理路径是否在应用程序目录内
                if (!physicalPath.StartsWith(appPhysicalPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new SecurityException("物理路径不在应用程序目录内，无法转换为虚拟路径");
                }

                // 移除应用程序物理路径前缀
                string relativePath = physicalPath.Substring(appPhysicalPath.Length);

                // 替换反斜杠为正斜杠，并移除开头的斜杠
                relativePath = relativePath.Replace('\\', '/').TrimStart('/');

                // 构建虚拟路径
                return $"~/{relativePath}";
            }
            catch (Exception ex)
            {
                throw new IOException($"物理路径转换失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 转换目录路径为HTTP可访问格式（替换分隔符）
        /// </summary>
        /// <param name="directory">目录</param>
        /// <returns></returns>
        public static string ConvertDirectoryToHttp(string directory)
        {
            directory = directory.ParseToString();
            directory = directory.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return directory;
        }

        /// <summary>
        /// 转换HTTP路径为目录格式（替换分隔符）
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        public static string ConvertHttpToDirectory(string http)
        {
            http = http.ParseToString();
            http = http.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            return http;
        }
        #endregion

        #region 文件基础操作
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="encoding">编码方式，默认UTF8</param>
        /// <returns>文件内容</returns>
        public static string ReadFile(string filePath, Encoding encoding = null)
        {
            try
            {
                encoding = encoding ?? Encoding.UTF8;
                return File.ReadAllText(filePath, encoding);
            }
            catch (Exception ex)
            {
                throw new IOException($"读取文件失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 写入文件内容
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="content">文件内容</param>
        /// <param name="encoding">编码方式，默认UTF8</param>
        /// <param name="append">是否追加内容</param>
        public static void WriteFile(string filePath, string content, Encoding encoding = null, bool append = false)
        {
            try
            {
                encoding = encoding ?? Encoding.UTF8;
                if (append)
                {
                    File.AppendAllText(filePath, content, encoding);
                }
                else
                {
                    File.WriteAllText(filePath, content, encoding);
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"写入文件失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 复制文件到指定目录
        /// </summary>
        /// <param name="sourceFilePath">源文件路径</param>
        /// <param name="destinationDirectory">目标目录</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        public static void CopyFileToDirectory(string sourceFilePath, string destinationDirectory, bool overwrite = false)
        {
            try
            {
                // 检查源文件是否存在
                if (!File.Exists(sourceFilePath))
                {
                    throw new FileNotFoundException($"源文件不存在: {sourceFilePath}");
                }

                // 检查目标目录是否存在，不存在则创建
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                // 获取文件名
                string fileName = Path.GetFileName(sourceFilePath);

                // 构建目标文件的完整路径
                string destinationFilePath = Path.Combine(destinationDirectory, fileName);

                // 复制文件
                File.Copy(sourceFilePath, destinationFilePath, overwrite);
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"文件复制失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 复制文件到指定文件夹，并返回复制后的完整文件路径
        /// </summary>
        /// <param name="sourceFilePath">源文件路径</param>
        /// <param name="destinationFolder">目标文件夹路径</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        /// <returns>复制后的完整文件路径</returns>
        public static string CopyFileToFolder(string sourceFilePath, string destinationFolder, bool overwrite = false)
        {
            string destinationFilePath = string.Empty;
            try
            {
                // 检查源文件是否存在
                if (!File.Exists(sourceFilePath))
                {
                    throw new FileNotFoundException($"源文件不存在: {sourceFilePath}");
                }

                // 检查目标文件夹是否存在，不存在则创建
                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                // 获取文件名
                string fileName = Path.GetFileName(sourceFilePath);
                // 构建目标文件的完整路径
                destinationFilePath = Path.Combine(destinationFolder, fileName);

                // 复制文件
                File.Copy(sourceFilePath, destinationFilePath, overwrite);

            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"文件复制失败: {ex.Message}", ex);
            }
            return destinationFilePath;
        }

        /// <summary>
        /// 批量复制文件到目标文件夹
        /// </summary>
        /// <param name="sourceFiles">源文件路径集合</param>
        /// <param name="destinationFolder">目标文件夹路径</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        /// <returns>复制成功的文件路径列表</returns>
        public static List<string> CopyFilesToFolder(IEnumerable<string> sourceFiles, string destinationFolder, bool overwrite = false)
        {
            List<string> copiedFiles = new List<string>();

            foreach (string sourceFile in sourceFiles)
            {
                try
                {
                    string copiedFile = CopyFileToFolder(sourceFile, destinationFolder, overwrite);
                    copiedFiles.Add(copiedFile);
                }
                catch (Exception ex)
                {
                    Log4netHelper.Error($"复制文件 {sourceFile} 时出错: {ex.Message}", ex);
                }
            }

            return copiedFiles;
        }

        /// <summary>
        /// 批量复制文件（并行）
        /// </summary>
        /// <param name="copyTasks">复制任务列表，包含源文件路径和目标目录</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        /// <param name="batchSize">每批次处理的文件数量（可选）</param>
        /// <returns>操作结果，包含状态、消息和详细结果</returns>
        public static TData BatchCopyFiles(List<(string source, string destination)> copyTasks, bool overwrite = false, int batchSize = 100)
        {
            var result = new TData
            {
                Status = 1,
                Message = "文件批量复制成功"
            };

            try
            {
                // 分批次处理
                for (int i = 0; i < copyTasks.Count; i += batchSize)
                {
                    var batch = copyTasks.Skip(i).Take(batchSize).ToList();

                    // 并行处理当前批次
                    Parallel.ForEach(batch, task =>
                    {
                        try
                        {
                            CopyFileToDirectory(task.source, task.destination, overwrite);
                        }
                        catch (Exception ex)
                        {
                            // 记录单个文件复制失败
                            result.Status = 0;
                            result.Message = $"部分文件复制失败: {ex.Message}";
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                result.Status = 0;
                result.Message = "文件批量复制失败: " + ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 递归复制目录（复制单个目录及其所有子目录和文件）
        /// </summary>
        /// <param name="sourceDir">源目录路径</param>
        /// <param name="targetDir">目标目录路径</param>
        /// <param name="copySubDirs">是否复制子目录</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static void DirectoryCopy(string sourceDir, string targetDir, bool copySubDirs = true)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("源目录不存在或无法访问: " + sourceDir);
            }

            // 统一创建目标目录
            FileHelper.EnsureDirectoryExists(targetDir);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(targetDir, file.Name);
                file.CopyTo(tempPath, true);
            }

            if (copySubDirs)
            {
                // 递归复制所有子目录
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(targetDir, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        /// <summary>
        /// 递归复制目录（复制单个目录及其所有子目录和文件）
        /// </summary>
        /// <param name="sourceDir">源目录路径</param>
        /// <param name="targetDir">目标目录路径</param>
        /// <param name="copySubDirs">是否复制子目录</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static void DirectoryCopy(string sourceDir, string targetDir, bool copySubDirs = true, bool overwrite = true)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("源目录不存在或无法访问: " + sourceDir);
            }

            // 统一创建目标目录
            FileHelper.EnsureDirectoryExists(targetDir);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(targetDir, file.Name);
                // 使用overwrite参数控制文件覆盖行为
                file.CopyTo(tempPath, overwrite);
            }

            if (copySubDirs)
            {
                // 递归复制所有子目录
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(targetDir, subdir.Name);
                    // 递归调用时传递相同的overwrite参数
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs, overwrite);
                }
            }
        }

        /// <summary>
        /// 批量复制多个目录及其子目录和文件
        /// </summary>
        /// <param name="sourceDirs">源目录路径列表</param>
        /// <param name="targetBaseDir">目标基础目录路径</param>
        /// <param name="copySubDirs">是否复制子目录</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        /// <returns>成功复制的目录数量</returns>
        public static void DirectoriesCopy(IEnumerable<string> sourceDirs, string targetBaseDir, bool copySubDirs = true, bool overwrite = true)
        {
            // 确保目标基础目录存在
            if (!Directory.Exists(targetBaseDir))
            {
                Directory.CreateDirectory(targetBaseDir);
            }

            foreach (string sourceDir in sourceDirs)
            {
                try
                {
                    // 提取源目录名称作为目标目录名称
                    string dirName = Path.GetFileName(sourceDir);
                    string targetDir = Path.Combine(targetBaseDir, dirName);

                    // 复制单个目录（使用扩展后的DirectoryCopy方法）
                    DirectoryCopy(sourceDir, targetDir, copySubDirs, overwrite);
                }
                catch (Exception ex)
                {
                    // 记录错误但继续处理其他目录
                    Log4netHelper.Warn($"复制目录 {sourceDir} 时出错: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 移动文件到指定目录
        /// </summary>
        /// <param name="sourceFilePath">源文件路径</param>
        /// <param name="destinationDirectory">目标目录</param>
        /// <returns>移动后的文件路径</returns>
        public static string MoveFileToDirectory(string sourceFilePath, string destinationDirectory)
        {
            try
            {
                // 确保目标目录存在
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                string fileName = Path.GetFileName(sourceFilePath);
                string destinationFilePath = Path.Combine(destinationDirectory, fileName);

                File.Move(sourceFilePath, destinationFilePath);
                return destinationFilePath;
            }
            catch (Exception ex)
            {
                throw new IOException($"移动文件失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 批量移动文件
        /// </summary>
        /// <param name="moveTasks">移动任务列表，包含源路径和目标路径</param>
        /// <param name="batchSize">每批次处理的文件数量（可选）</param>
        /// <returns>操作结果，包含状态、消息和详细结果</returns>
        public static TData BatchMoveFiles(List<(string source, string target)> moveTasks, int batchSize = 100)
        {
            var result = new TData<bool>
            {
                Status = 1,
                Message = "文件批量移动成功"
            };

            try
            {
                // 分批次处理
                for (int i = 0; i < moveTasks.Count; i += batchSize)
                {
                    var batch = moveTasks.Skip(i).Take(batchSize).ToList();

                    // 并行处理当前批次
                    Parallel.ForEach(batch, task =>
                    {
                        try
                        {
                            MoveFileToDirectory(task.source, task.target);
                        }
                        catch (Exception ex)
                        {
                            // 记录单个文件移动失败
                            result.Status = 0;
                            result.Message = $"部分文件移动失败: {ex.Message}";
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                result.Status = 0;
                result.Message = "文件批量移动失败: " + ex.Message;
            }
            return result;
        }
        #endregion

        #region 目录操作
        /// <summary>
        /// 判断目录是否存在，不存在则创建
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        public static void CreateDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// 判断目录是否存在，不存在则创建（支持文件路径，自动提取目录）
        /// </summary>
        /// <param name="directoryPath">目录路径（支持文件路径，自动提取目录）</param>
        public static void EnsureDirectoryExists(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath)) return;

            // 如果是文件路径，提取所在目录
            string targetDir = Path.HasExtension(directoryPath) ? Path.GetDirectoryName(directoryPath) : directoryPath;

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
        }

        /// <summary>
        /// 删除目录及其下所有文件和子目录（递归删除）
        /// </summary>
        /// <param name="filePath">文件夹路径</param>
        public static void DeleteDirectory(string filePath)
        {
            try
            {
                if (Directory.Exists(filePath)) //如果存在这个文件夹删除之 
                {
                    foreach (string d in Directory.GetFileSystemEntries(filePath))
                    {
                        if (File.Exists(d))
                            File.Delete(d); //直接删除其中的文件                        
                        else
                            DeleteDirectory(d); //递归删除子文件夹 
                    }
                    Directory.Delete(filePath, true); //删除已空文件夹                 
                }
            }
            catch (Exception ex)
            {
                Log4netHelper.Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// 获取目录中的所有文件
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <param name="searchPattern">搜索模式，如"*.txt"</param>
        /// <param name="searchOption">搜索选项</param>
        /// <returns>文件路径数组</returns>
        public static string[] GetFiles(string directoryPath, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            try
            {
                return Directory.GetFiles(directoryPath, searchPattern, searchOption);
            }
            catch (Exception ex)
            {
                throw new IOException($"获取文件列表失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取目录信息
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <returns>目录信息</returns>
        public static DirectoryInfo GetDirectoryInfo(string directoryPath)
        {
            try
            {
                return new DirectoryInfo(directoryPath);
            }
            catch (Exception ex)
            {
                throw new IOException($"获取目录信息失败: {ex.Message}", ex);
            }
        }
        #endregion

        #region 文件信息操作
        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件信息</returns>
        public static FileInfo GetFileInfo(string filePath)
        {
            try
            {
                return new FileInfo(filePath);
            }
            catch (Exception ex)
            {
                throw new IOException($"获取文件信息失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取指定文件详细属性
        /// </summary>
        /// <param name="filePath">文件详细路径</param>
        /// <returns></returns>
        public static string GetFileAttibe(string filePath)
        {
            string str = "";
            FileInfo objFI = new FileInfo(filePath);
            str += "详细路径:" + objFI.FullName + "<br>文件名称:" + objFI.Name + "<br>文件长度:" + objFI.Length.ToString() + "字节<br>创建时间" + objFI.CreationTime.ToString() + "<br>最后访问时间:" + objFI.LastAccessTime.ToString() + "<br>修改时间:" + objFI.LastWriteTime.ToString() + "<br>所在目录:" + objFI.DirectoryName + "<br>扩展名:" + objFI.Extension;
            return str;
        }

        /// <summary>
        /// 获取文件大小（格式化）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>格式化的文件大小字符串</returns>
        public static string GetFormattedFileSize(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                return FormatFileSize(fileInfo.Length);
            }
            catch (Exception ex)
            {
                throw new IOException($"获取文件大小失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取文件夹大小
        /// </summary>
        /// <param name="dirPath">文件夹路径</param>
        /// <returns></returns>
        public static long GetDirectoryLength(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                return 0;
            long len = 0;
            DirectoryInfo di = new DirectoryInfo(dirPath);
            foreach (FileInfo fi in di.GetFiles())
            {
                len += fi.Length;
            }
            DirectoryInfo[] dis = di.GetDirectories();
            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectoryLength(dis[i].FullName);
                }
            }
            return len;
        }

        /// <summary>
        /// 格式化文件大小
        /// </summary>
        /// <param name="bytes">字节数</param>
        /// <returns>格式化的大小字符串</returns>
        public static string FormatFileSize(long bytes)
        {
            if (bytes >= 1024 * 1024 * 1024) // GB
                return $"{bytes / (1024.0 * 1024 * 1024):0.##} GB";
            if (bytes >= 1024 * 1024) // MB
                return $"{bytes / (1024.0 * 1024):0.##} MB";
            if (bytes >= 1024) // KB
                return $"{bytes / 1024.0:0.##} KB";
            return $"{bytes} bytes";
        }

        /// <summary>
        /// 获取文件MIME类型
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>MIME类型</returns>
        public static string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath)?.ToLowerInvariant() ?? string.Empty;

            var mimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".txt", "text/plain" },
                { ".pdf", "application/pdf" },
                { ".doc", "application/msword" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".gif", "image/gif" },
                { ".bmp", "image/bmp" },
                { ".zip", "application/zip" },
                { ".rar", "application/x-rar-compressed" },
                { ".7z", "application/x-7z-compressed" },
                { ".json", "application/json" },
                { ".xml", "application/xml" },
                { ".csv", "text/csv" },
                { ".html", "text/html" },
                { ".htm", "text/html" }
            };

            return mimeTypes.TryGetValue(extension, out string mimeType) ? mimeType : "application/octet-stream";
        }
        #endregion

        #region 高级操作

        /// <summary>
        /// 合并多个文件
        /// </summary>
        /// <param name="sourceFiles">源文件路径列表</param>
        /// <param name="destinationFile">目标文件路径</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        public static void MergeFiles(IEnumerable<string> sourceFiles, string destinationFile, bool overwrite = false)
        {
            try
            {
                if (!overwrite && File.Exists(destinationFile))
                    throw new IOException($"文件已存在: {destinationFile}");

                using (var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write))
                {
                    foreach (var sourceFile in sourceFiles)
                    {
                        using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                        {
                            sourceStream.CopyTo(destinationStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"合并文件失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 分块读取大文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="chunkSize">块大小（字节）</param>
        /// <param name="processChunk">处理每个块的委托</param>
        public static void ReadFileInChunks(string filePath, int chunkSize = 1024 * 1024, Action<byte[], int> processChunk = null)
        {
            if (chunkSize <= 0)
                throw new ArgumentException("块大小必须大于0", nameof(chunkSize));

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var buffer = new byte[chunkSize];
                    int bytesRead;

                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        processChunk?.Invoke(buffer, bytesRead);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"分块读取文件失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 计算文件哈希值（MD5）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>MD5哈希值</returns>
        public static string GetFileHashMD5(string filePath)
        {
            try
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    var hashBytes = md5.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"计算文件MD5失败: {ex.Message}", ex);
            }
        }

        #endregion

        #region 目录名、文件名和扩展名操作

        /// <summary>
        /// 安全提取目录路径的最后一级目录名
        /// </summary>
        /// <param name="dirPath">目录路径</param>
        /// <returns>最后一级目录名</returns>
        public static string GetDirectoryName(string dirPath)
        {
            // 确保路径不为空
            if (string.IsNullOrEmpty(dirPath))
            {
                throw new ArgumentException("目录路径不能为空");
            }

            // 移除路径末尾的分隔符
            string cleanPath = dirPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // 提取最后一级目录名
            return Path.GetFileName(cleanPath);
        }

        /// <summary>
        /// 获取文件名（不带扩展名）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件名（不带扩展名）</returns>
        public static string GetFileNameWithoutExtension(string filePath)
        {
            try
            {
                return Path.GetFileNameWithoutExtension(filePath);
            }
            catch (Exception ex)
            {
                throw new IOException($"获取文件名（不带扩展名）失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取文件名（带扩展名）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件名（带扩展名）</returns>
        public static string GetFileNameWithExtension(string filePath)
        {
            try
            {
                return Path.GetFileName(filePath);
            }
            catch (Exception ex)
            {
                throw new IOException($"获取文件名（带扩展名）失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取文件扩展名（包含点号，如".txt"）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件扩展名</returns>
        public static string GetFileExtension(string filePath)
        {
            try
            {
                string extension = Path.GetExtension(filePath);
                return string.IsNullOrEmpty(extension) ? string.Empty : extension.ToLowerInvariant();
            }
            catch (Exception ex)
            {
                throw new IOException($"获取文件扩展名失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取文件扩展名（不包含点号，如"txt"）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件扩展名（不包含点号）</returns>
        public static string GetFileExtensionWithoutDot(string filePath)
        {
            try
            {
                string extension = Path.GetExtension(filePath);
                if (string.IsNullOrEmpty(extension))
                    return string.Empty;

                return extension.TrimStart('.').ToLowerInvariant();
            }
            catch (Exception ex)
            {
                throw new IOException($"获取文件扩展名（不包含点号）失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 更改文件扩展名
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="newExtension">新的扩展名（可以包含或不包含点号）</param>
        /// <returns>更改扩展名后的新路径</returns>
        public static string ChangeFileExtension(string filePath, string newExtension)
        {
            try
            {
                // 确保扩展名以点号开头
                if (!newExtension.StartsWith("."))
                {
                    newExtension = "." + newExtension;
                }

                return Path.ChangeExtension(filePath, newExtension);
            }
            catch (Exception ex)
            {
                throw new IOException($"更改文件扩展名失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查文件扩展名
        /// </summary>
        /// <param name="fileExtension">文件扩展名</param>
        /// <param name="allowedExtensions">允许的扩展名列表</param>
        /// <returns></returns>
        public static TData CheckFileExtension(string fileExtension, string allowedExtensions)
        {
            TData obj = new TData();
            string[] allowArr = TextHelper.SplitToArray<string>(allowedExtensions.ToLower(), '|');
            if (allowArr.Where(p => p.Trim() == fileExtension.ParseToString().ToLower()).Any())
            {
                obj.Status = 1;
            }
            else
            {
                obj.Message = "只有文件扩展名是 " + allowedExtensions + " 的文件才能上传";
            }
            return obj;
        }

        /// <summary>
        /// 检查文件扩展名，使用逗号分隔的字符串指定允许的扩展名（如".jpg,.jpeg,.png"）
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="allowedExtensions">允许的扩展名列表，用逗号分隔（如".jpg,.jpeg,.png"）</param>
        /// <returns>是否有效</returns>
        public static bool IsValidFileExtension(string fileName, string allowedExtensions)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(allowedExtensions))
                return false;

            try
            {
                string extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;

                // 分割允许的扩展名，移除空格，并转换为小写
                var validExtensions = allowedExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(ext => ext.Trim().ToLowerInvariant())
                    .ToList();

                return validExtensions.Any(ext =>
                    ext.Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals(extension.TrimStart('.'), StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 检查文件扩展名，使用可变参数列表指定允许的扩展名（如".jpg", ".png"等）
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="allowedExtensions">允许的扩展名列表</param>
        /// <returns>是否有效</returns>
        public static bool IsValidFileExtension(string fileName, params string[] allowedExtensions)
        {
            if (string.IsNullOrWhiteSpace(fileName) || allowedExtensions == null || allowedExtensions.Length == 0)
                return false;

            string extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;
            return allowedExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 检查文件扩展名，使用指定分隔符的字符串指定允许的扩展名（如".jpg|.jpeg|.png"），分隔符默认为逗号
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="allowedExtensions">允许的扩展名列表，用指定分隔符分隔（如".jpg|.jpeg|.png"）</param>
        /// <param name="separator">分隔符，默认为逗号</param>
        /// <returns>是否有效</returns>
        public static bool IsValidFileExtension(string fileName, string allowedExtensions, char separator = ',')
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(allowedExtensions))
                return false;

            try
            {
                string extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;

                // 分割允许的扩展名，移除空格，并转换为小写
                var validExtensions = allowedExtensions.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(ext => ext.Trim().ToLowerInvariant())
                    .ToList();

                return validExtensions.Any(ext =>
                    ext.Equals(extension, StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals(extension.TrimStart('.'), StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region 安全和验证
        /// <summary>
        /// 过滤文件路径中的非法字符（如../、..、~等），防止目录遍历攻击
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string FilterFilePath(string filePath)
        {
            filePath = filePath.Replace("../", string.Empty);
            filePath = filePath.Replace("..", string.Empty);
            filePath = filePath.Replace("~", string.Empty);
            filePath = filePath.Replace("~/", string.Empty);
            filePath = filePath.TrimStart('/');
            return filePath;
        }
        #endregion
    }
}
