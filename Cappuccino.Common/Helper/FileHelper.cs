using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Cappuccino.Common.Enum;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Log;
using Cappuccino.Common.Util;
using log4net;

namespace Cappuccino.Common.Helper
{
    public class FileHelper
    {
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

        /// <summary>
        /// 判断目录是否存在，不存在则创建
        /// </summary>
        /// <param name="directory">目录路径</param>
        public static void CreateDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
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
            string targetDir = Path.HasExtension(directoryPath)
                ? Path.GetDirectoryName(directoryPath)
                : directoryPath;

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
        }

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

        public static string ConvertDirectoryToHttp(string directory)
        {
            directory = directory.ParseToString();
            directory = directory.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return directory;
        }

        public static string ConvertHttpToDirectory(string http)
        {
            http = http.ParseToString();
            http = http.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            return http;
        }

        /// <summary>
        /// 检查文件扩展名
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <param name="allowExtension"></param>
        /// <returns></returns>
        public static TData CheckFileExtension(string fileExtension, string allowExtension)
        {
            TData obj = new TData();
            string[] allowArr = TextHelper.SplitToArray<string>(allowExtension.ToLower(), '|');
            if (allowArr.Where(p => p.Trim() == fileExtension.ParseToString().ToLower()).Any())
            {
                obj.Status = 1;
            }
            else
            {
                obj.Message = "只有文件扩展名是 " + allowExtension + " 的文件才能上传";
            }
            return obj;
        }

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

        /// <summary>
        /// 获取物理路径（转换虚拟路径）
        /// </summary>
        public static string GetPhysicalPath(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath)) return string.Empty;
            return HttpContext.Current?.Server.MapPath(virtualPath) ?? HostingEnvironment.MapPath(virtualPath);
        }
    }
}
