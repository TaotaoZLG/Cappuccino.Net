using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cappuccino.Common.Extensions;
using MiniExcelLibs;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;

namespace Cappuccino.Common.Helper
{
    public static class CompressHelper
    {
        /// <summary>
        /// 支持的压缩包格式
        /// </summary>
        private static readonly List<string> SupportCompressExts = new List<string> { ".zip", ".rar", ".7z" };
        /// <summary>
        /// 支持的图片格式
        /// </summary>
        private static readonly List<string> SupportImageExts = new List<string> { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff" };

        /// <summary>
        /// 解压压缩包（递归处理目录层级）
        /// </summary>
        /// <param name="compressFilePath">压缩包路径</param>
        /// <param name="unzipDir">解压目录</param>
        /// <param name="batchId">批次ID</param>
        /// <param name="progressAction">进度回调</param>
        /// <returns>解压后的所有文件路径</returns>
        public static async Task<List<string>> UnzipFileAsync(string compressFilePath, string unzipDir, string batchId, Action<ProcessProgress> progressAction)
        {
            FileHelper.EnsureDirectoryExists(unzipDir);    

            return await Task.Run(() =>
            {
                if (!File.Exists(compressFilePath))
                {
                    throw new FileNotFoundException("压缩包文件不存在", compressFilePath);
                }
                var fileExt = Path.GetExtension(compressFilePath).ToLower();
                if (!SupportCompressExts.Contains(fileExt))
                {
                    throw new NotSupportedException($"不支持的压缩格式：{fileExt}，仅支持{string.Join(",", SupportCompressExts)}");
                }

                FileHelper.EnsureDirectoryExists(unzipDir);

                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 10,
                    Type = "Unzip",
                    Message = $"开始解压压缩包：{Path.GetFileName(compressFilePath)}"
                });

                List<string> allFiles = new List<string>();
                IArchive archive = null;
                try
                {
                    switch (fileExt)
                    {
                        case ".zip":
                            archive = ZipArchive.Open(compressFilePath);
                            break;
                        case ".rar":
                            archive = RarArchive.Open(compressFilePath);
                            break;
                        case ".7z":
                            archive = SevenZipArchive.Open(compressFilePath);
                            break;
                        default:
                            throw new InvalidOperationException("无法识别的压缩包格式");
                    }

                    if (archive == null)
                    {
                        throw new InvalidOperationException("无法识别的压缩包格式");
                    }

                    // 递归获取所有文件（含子目录）
                    var entries = archive.Entries.Where(x => !x.IsDirectory).ToList();
                    int total = entries.Count;
                    int processed = 0;

                    foreach (var entry in entries)
                    {
                        var entryFilePath = Path.Combine(unzipDir, entry.Key);
                        // 统一创建子目录
                        FileHelper.EnsureDirectoryExists(entryFilePath);

                        // 解压文件
                        entry.WriteToFile(entryFilePath, new ExtractionOptions
                        {
                            Overwrite = true,
                            ExtractFullPath = true
                        });
                        allFiles.Add(entryFilePath);
                        processed++;

                        // 更新进度
                        int progress = 10 + (int)((processed / (double)total) * 20);
                        progressAction.Invoke(new ProcessProgress
                        {
                            BatchId = batchId,
                            Progress = progress,
                            Type = "Unzip",
                            Message = $"已解压：{entry.Key}（{processed}/{total}）"
                        });
                    }

                    progressAction.Invoke(new ProcessProgress
                    {
                        BatchId = batchId,
                        Progress = 30,
                        Type = "Unzip",
                        Message = $"压缩包解压完成，共解压{total}个文件"
                    });
                    return allFiles;
                }
                finally
                {
                    archive?.Dispose();
                }
            });
        }

        /// <summary>
        /// 根据归档规则归档文件（相同规则文件名的文件存放到同一个文件夹）
        /// 新增：如果文件只有一层目录，直接复制当前目录层级到归档文件夹
        /// </summary>
        /// <param name="filePaths">待归档的文件路径</param>
        /// <param name="archiveRootDir">归档根目录</param>
        /// <param name="rule">归档规则</param>
        /// <param name="batchId">批次ID</param>
        /// <param name="progressAction">进度回调</param>
        /// <returns>归档后的目录字典（文件夹名→文件列表）</returns>
        public static async Task<Dictionary<string, List<string>>> ArchiveFilesAsync(List<string> filePaths, string archiveRootDir, FileArchiveRuleEnum rule, string batchId, Action<ProcessProgress> progressAction)
        {
            // 统一创建目录
            FileHelper.EnsureDirectoryExists(archiveRootDir);

            return await Task.Run(() =>
            {
                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 35,
                    Type = "Archive",
                    Message = $"开始按规则归档文件，规则：{rule.ToString()}"
                });

                // 【新增】检测是否为单层目录结构
                bool isSingleLayer = filePaths.IsSingleLayerDirectory();

                if (isSingleLayer && filePaths.Count > 0)
                {
                    // 单层目录：直接复制整个目录结构
                    string sourceParentDir = Path.GetDirectoryName(filePaths[0]);
                    string sourceParentDirName = Path.GetFileName(sourceParentDir);

                    // 处理目标文件夹名（避免重名）
                    string targetFolderName = sourceParentDirName;
                    string targetFolderPath = Path.Combine(archiveRootDir, targetFolderName);
                    int index = 1;
                    while (Directory.Exists(targetFolderPath))
                    {
                        targetFolderName = $"{sourceParentDirName}_{index}";
                        index++;
                    }
                    FileHelper.EnsureDirectoryExists(targetFolderPath);

                    progressAction.Invoke(new ProcessProgress
                    {
                        BatchId = batchId,
                        Progress = 40,
                        Type = "Archive",
                        Message = $"检测到单层目录结构，直接复制目录：{sourceParentDirName}"
                    });

                    // 复制所有文件（保持原有子目录结构）
                    List<string> archivedFilePaths = new List<string>();
                    int total = filePaths.Count;
                    int processed = 0;

                    foreach (var filePath in filePaths)
                    {
                        // 计算相对路径（保持原有子目录结构）
                        string relativePath = PathExtensions.GetRelativePath(sourceParentDir, filePath);
                        string targetFilePath = Path.Combine(targetFolderPath, relativePath);

                        // 确保目标文件的目录存在
                        FileHelper.EnsureDirectoryExists(targetFilePath);

                        // 复制文件
                        File.Copy(filePath, targetFilePath, true);
                        archivedFilePaths.Add(targetFilePath);

                        processed++;
                        int progress = 40 + (int)((processed / (double)total) * 5);
                        progressAction.Invoke(new ProcessProgress
                        {
                            BatchId = batchId,
                            Progress = progress,
                            Type = "Archive",
                            Message = $"已复制：{relativePath}（{processed}/{total}）"
                        });
                    }

                    progressAction.Invoke(new ProcessProgress
                    {
                        BatchId = batchId,
                        Progress = 45,
                        Type = "Archive",
                        Message = $"单层目录归档完成，共复制{total}个文件到：{targetFolderName}"
                    });

                    return new Dictionary<string, List<string>> { { targetFolderName, archivedFilePaths } };
                }
                else
                {
                    // 多层目录或扁平文件：按原有规则分组归档
                    #region 需求1核心：先按规则分组，同组文件放入同一文件夹
                    // 1. 按归档规则分组，key=目标文件夹名，value=同组文件路径列表
                    Dictionary<string, List<string>> groupDict = new Dictionary<string, List<string>>();
                    foreach (var filePath in filePaths)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        string targetFolderName = GetTargetFolderName(fileName, rule);

                        // 同规则文件名分到同一组
                        if (!groupDict.ContainsKey(targetFolderName))
                        {
                            groupDict.Add(targetFolderName, new List<string>());
                        }
                        groupDict[targetFolderName].Add(filePath);
                    }

                    // 2. 处理重复文件夹名（整组加序号，而非单文件）
                    Dictionary<string, List<string>> archiveDict = new Dictionary<string, List<string>>();
                    foreach (var groupItem in groupDict)
                    {
                        string targetFolderName = groupItem.Key;
                        List<string> groupFiles = groupItem.Value;

                        // 处理重名
                        string finalFolderName = targetFolderName;
                        int index = 1;
                        while (Directory.Exists(Path.Combine(archiveRootDir, finalFolderName)))
                        {
                            finalFolderName = $"{targetFolderName}_{index}";
                            index++;
                        }

                        // 创建归档文件夹
                        string targetFolderPath = Path.Combine(archiveRootDir, finalFolderName);
                        FileHelper.EnsureDirectoryExists(targetFolderPath);

                        // 复制同组所有文件到该文件夹
                        List<string> archivedFilePaths = new List<string>();
                        foreach (var filePath in groupFiles)
                        {
                            string targetFilePath = Path.Combine(targetFolderPath, Path.GetFileName(filePath));
                            File.Copy(filePath, targetFilePath, true);
                            archivedFilePaths.Add(targetFilePath);
                        }

                        // 加入结果字典
                        archiveDict.Add(finalFolderName, archivedFilePaths);
                    }
                    #endregion

                    progressAction.Invoke(new ProcessProgress
                    {
                        BatchId = batchId,
                        Progress = 45,
                        Type = "Archive",
                        Message = $"文件归档完成，共创建{archiveDict.Count}个文件夹"
                    });
                    return archiveDict;
                }
            });
        }

        /// <summary>
        /// 检测文件列表是否为单层目录结构（所有文件的父目录相同）
        /// </summary>
        /// <param name="filePaths">文件路径列表</param>
        /// <returns>true=单层目录，false=多层目录或扁平文件</returns>
        public static bool IsSingleLayerDirectory(this List<string> filePaths)
        {
            if (filePaths == null || filePaths.Count == 0)
            {
                return false;
            }

            // 获取所有文件的父目录
            var parentDirs = filePaths
                .Where(f => !string.IsNullOrEmpty(f))
                .Select(f => Path.GetDirectoryName(f))
                .Distinct()
                .ToList();

            // 如果所有文件的父目录相同，则为单层目录
            return parentDirs.Count == 1;
        }
        /// <summary>
        /// 根据规则获取目标文件夹名（保留原有自动降级规则）
        /// </summary>
        /// <param name="fileName">文件名（无扩展名）</param>
        /// <param name="rule">归档规则</param>
        /// <returns>目标文件夹名</returns>
        private static string GetTargetFolderName(string fileName, FileArchiveRuleEnum rule)
        {
            string folderName = fileName;
            // 兼容C#7.3 传统switch写法
            switch (rule)
            {
                case FileArchiveRuleEnum.SystemDefault:
                    // 自动降级：规则1→规则2→规则3
                    folderName = GetUnderlineFirstSegment(fileName);
                    if (string.IsNullOrEmpty(folderName) || folderName == fileName)
                    {
                        folderName = GetNameIdCardSegment(fileName);
                    }
                    if (string.IsNullOrEmpty(folderName) || folderName == fileName)
                    {
                        folderName = fileName;
                    }
                    break;
                case FileArchiveRuleEnum.UnderlineFirstSegment:
                    folderName = GetUnderlineFirstSegment(fileName);
                    break;
                case FileArchiveRuleEnum.NameIdCard:
                    folderName = GetNameIdCardSegment(fileName);
                    break;
                case FileArchiveRuleEnum.OriginalFileName:
                    folderName = fileName;
                    break;
                default:
                    folderName = fileName;
                    break;
            }
            // 过滤非法字符
            folderName = SanitizeFolderName(folderName);
            return folderName;
        }

        /// <summary>
        /// 下划线分隔取首段
        /// </summary>
        private static string GetUnderlineFirstSegment(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return fileName;
            }
            var segments = fileName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            return segments.Length > 0 ? segments[0] : fileName;
        }

        /// <summary>
        /// 提取姓名+身份证号/身份证号+姓名
        /// </summary>
        private static string GetNameIdCardSegment(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return fileName;
            }
            // 匹配身份证号（18位）
            var idCardRegex = new Regex(@"\d{17}[\dXx]");
            var idCardMatch = idCardRegex.Match(fileName);
            if (idCardMatch.Success)
            {
                return idCardMatch.Value;
            }
            // 匹配姓名（中文）+身份证号 格式
            var nameIdCardRegex = new Regex(@"([\u4e00-\u9fa5]+)[-_]?(\d{17}[\dXx])|(\d{17}[\dXx])[-_]?([\u4e00-\u9fa5]+)");
            var nameIdCardMatch = nameIdCardRegex.Match(fileName);
            if (nameIdCardMatch.Success)
            {
                return nameIdCardMatch.Value;
            }
            return fileName;
        }

        /// <summary>
        /// 过滤文件夹名非法字符
        /// </summary>
        private static string SanitizeFolderName(string folderName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                folderName = folderName.Replace(c.ToString(), "_");
            }
            return folderName.Trim();
        }

        /// <summary>
        /// 过滤有效图片文件
        /// </summary>
        /// <param name="filePaths">文件路径列表</param>
        /// <param name="batchId">批次ID</param>
        /// <param name="progressAction">进度回调</param>
        /// <returns>有效图片文件路径</returns>
        public static async Task<List<string>> FilterImageFilesAsync(List<string> filePaths, string batchId, Action<ProcessProgress> progressAction)
        {
            return await Task.Run(() =>
            {
                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 50,
                    Type = "Filter",
                    Message = "开始过滤有效图片文件"
                });

                List<string> imageFiles = new List<string>();
                int total = filePaths.Count;
                int processed = 0;

                foreach (var filePath in filePaths)
                {
                    string fileExt = Path.GetExtension(filePath).ToLower();
                    if (SupportImageExts.Contains(fileExt))
                    {
                        imageFiles.Add(filePath);
                    }
                    processed++;

                    int progress = 50 + (int)((processed / (double)total) * 5);
                    progressAction.Invoke(new ProcessProgress
                    {
                        BatchId = batchId,
                        Progress = progress,
                        Type = "Filter",
                        Message = $"已过滤：{Path.GetFileName(filePath)}（有效图片：{imageFiles.Count}/{processed}）"
                    });
                }

                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 55,
                    Type = "Filter",
                    Message = $"图片过滤完成，共筛选出{imageFiles.Count}个有效图片文件"
                });
                return imageFiles;
            });
        }

        /// <summary>
        /// 生成OCR识别结果Excel
        /// </summary>
        /// <param name="ocrResultDict">OCR结果字典（文件夹名→(图片路径,识别文本)）</param>
        /// <param name="excelFilePath">Excel保存路径</param>
        /// <param name="batchId">批次ID</param>
        /// <param name="progressAction">进度回调</param>
        public static async Task GenerateOcrExcelAsync(Dictionary<string, Dictionary<string, string>> ocrResultDict, string excelFilePath, string batchId, Action<ProcessProgress> progressAction)
        {
            // 修复bug：统一创建Excel所在目录，而非判断文件是否为目录
            FileHelper.EnsureDirectoryExists(excelFilePath);

            await Task.Run(() =>
            {
                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 60,
                    Type = "Excel",
                    Message = "开始生成OCR识别结果Excel文件"
                });

                // 保存Excel文件
                using (FileStream fs = new FileStream(excelFilePath, FileMode.Create, FileAccess.Write))
                {
                    // 创建Excel工作簿
                    IWorkbook workbook = new XSSFWorkbook();
                    ISheet sheet = workbook.CreateSheet("OCR识别结果");

                    // 创建表头
                    IRow headerRow = sheet.CreateRow(0);
                    headerRow.CreateCell(0).SetCellValue("文件夹名称");
                    headerRow.CreateCell(1).SetCellValue("图片文件名");
                    headerRow.CreateCell(2).SetCellValue("识别文本内容");

                    int rowIndex = 1;
                    int total = ocrResultDict.Sum(x => x.Value.Count);
                    int processed = 0;

                    // 填充数据
                    foreach (var folderItem in ocrResultDict)
                    {
                        string folderName = folderItem.Key;
                        foreach (var imageItem in folderItem.Value)
                        {
                            string imageFileName = Path.GetFileName(imageItem.Key);
                            string ocrText = imageItem.Value;

                            IRow row = sheet.CreateRow(rowIndex);
                            row.CreateCell(0).SetCellValue(folderName);
                            row.CreateCell(1).SetCellValue(imageFileName);
                            row.CreateCell(2).SetCellValue(ocrText);

                            rowIndex++;
                            processed++;

                            int progress = 60 + (int)((processed / (double)total) * 20);
                            progressAction.Invoke(new ProcessProgress
                            {
                                BatchId = batchId,
                                Progress = progress,
                                Type = "Excel",
                                Message = $"已填充：{folderName}/{imageFileName}（{processed}/{total}）"
                            });
                        }
                    }

                    // 自动调整列宽
                    for (int i = 0; i < 3; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }

                    workbook.Write(fs);
                    workbook.Close();
                }

                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 80,
                    Type = "Excel",
                    Message = $"Excel文件生成完成，路径：{excelFilePath}"
                });
            });
        }

        /// <summary>
        /// 根据实体列表生成Excel文件（MiniExcel）
        /// </summary>
        /// <param name="dataList">实体列表</param>
        /// <param name="excelPath">生成的Excel路径</param>
        /// <param name="batchId">批次ID</param>
        /// <param name="progressAction">进度回调</param>
        public static async Task GenerateExcelFromEntityAsync(object dataList, string excelPath, string batchId, Action<ProcessProgress> progressAction)
        {
            try
            {
                // MiniExcel写入Excel（自动映射中文列头）
                await MiniExcel.SaveAsAsync(excelPath, dataList);
                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 80,
                    Type = "Finish",
                    Message = $"成功生成Excel文件：{Path.GetFileName(excelPath)}"
                });
            }
            catch (Exception ex)
            {
                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 80,
                    Type = "Error",
                    Message = $"生成Excel文件失败：{ex.Message}"
                });
                throw;
            }
        }

        /// <summary>
        /// 打包文件夹为ZIP（归档文件夹+Excel统一打包）
        /// </summary>
        /// <param name="sourceDir">待打包目录（虚拟路径）</param>
        /// <param name="outputZipPath">输出ZIP路径（虚拟路径）</param>
        /// <param name="batchId">批次ID</param>
        /// <param name="progressAction">进度回调</param>
        public static async Task PackFolderToZipAsync(string sourceDir, string outputZipPath, string batchId, Action<ProcessProgress> progressAction)
        {
            // 【新增】将虚拟路径转换为物理路径
            string sourceDirPhysical = FileHelper.GetPhysicalPath(sourceDir);
            string outputZipPathPhysical = FileHelper.GetPhysicalPath(outputZipPath);

            // 确保输出目录存在 (使用物理路径)
            FileHelper.EnsureDirectoryExists(outputZipPathPhysical);

            await Task.Run(() =>
            {
                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 85,
                    Type = "Pack",
                    Message = $"开始打包目录：{Path.GetFileName(sourceDir)}"
                });

                // 删除已存在的ZIP文件 (使用物理路径)
                if (File.Exists(outputZipPathPhysical))
                {
                    File.Delete(outputZipPathPhysical);
                }

                // 检查源目录是否存在 (使用物理路径)
                if (!Directory.Exists(sourceDirPhysical))
                {
                    throw new DirectoryNotFoundException($"源目录不存在：{sourceDirPhysical}");
                }

                // 打包目录
                using (var archive = ZipArchive.Create())
                {
                    var allFiles = Directory.GetFiles(sourceDirPhysical, "*.*", SearchOption.AllDirectories);
                    int total = allFiles.Length;
                    int processed = 0;

                    foreach (var file in allFiles)
                    {
                        // 计算相对路径 (使用物理路径进行截取)
                        string relativePath = file.Substring(sourceDirPhysical.Length).TrimStart(Path.DirectorySeparatorChar);
                        archive.AddEntry(relativePath, file);
                        processed++;

                        int progress = 85 + (int)((processed / (double)total) * 10);
                        progressAction.Invoke(new ProcessProgress
                        {
                            BatchId = batchId,
                            Progress = progress,
                            Type = "Pack",
                            Message = $"已打包：{relativePath}（{processed}/{total}）"
                        });
                    }
                    // 保存到物理路径
                    archive.SaveTo(outputZipPathPhysical, CompressionType.Deflate);
                }

                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 95,
                    Type = "Pack",
                    Message = $"打包完成，文件路径：{outputZipPath}"
                });
            });
        }

        /// <summary>
        /// 清理临时目录
        /// </summary>
        /// <param name="tempDir">临时目录</param>
        /// <param name="batchId">批次ID</param>
        /// <param name="progressAction">进度回调</param>
        public static async Task CleanTempDirAsync(string tempDir, string batchId, Action<ProcessProgress> progressAction)
        {
            await Task.Run(() =>
            {
                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                        progressAction.Invoke(new ProcessProgress
                        {
                            BatchId = batchId,
                            Progress = 100,
                            Type = "Finish",
                            Message = $"临时目录已清理：{tempDir}"
                        });
                    }
                    catch (Exception ex)
                    {
                        progressAction.Invoke(new ProcessProgress
                        {
                            BatchId = batchId,
                            Progress = 100,
                            Type = "Error",
                            Message = $"清理临时目录失败：{ex.Message}"
                        });
                    }
                }
            });
        }

        /// <summary>
        /// 递归复制目录
        /// </summary>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("源目录不存在或无法访问: " + sourceDirName);
            }

            // 统一创建目标目录
            FileHelper.EnsureDirectoryExists(destDirName);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            if (copySubDirs)
            {
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        /// <summary>
        /// 批量复制文件（覆盖已存在文件）
        /// </summary>
        public static void CopyFiles(IEnumerable<string> files, string targetDir)
        {
            foreach (var file in files)
            {
                string destFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }
        }
    }

    /// <summary>
    /// 处理进度模型
    /// </summary>
    public class ProcessProgress
    {
        /// <summary>
        /// 批次ID
        /// </summary>
        public string BatchId { get; set; }

        /// <summary>
        /// 进度百分比（0-100）
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// 进度类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 进度消息
        /// </summary>
        public string Message { get; set; }
    }


    /// <summary>
    /// 文件归档规则枚举
    /// </summary>
    public enum FileArchiveRuleEnum
    {
        /// <summary>
        /// 系统默认（自动降级规则1-3）
        /// </summary>
        SystemDefault = 0,
        /// <summary>
        /// 下划线分隔取首段
        /// </summary>
        UnderlineFirstSegment = 1,
        /// <summary>
        /// 姓名+身份证号/身份证号+姓名
        /// </summary>
        NameIdCard = 2,
        /// <summary>
        /// 源文件名兜底
        /// </summary>
        OriginalFileName = 3
    }
}