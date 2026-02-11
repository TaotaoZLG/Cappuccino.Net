using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Cappuccino.Common.Util;
using HashLib;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace Cappuccino.Common.Helper
{
    /// <summary>
    /// 压缩包解压+文件过滤工具类
    /// 贴合项目现有工具类风格（如VerifyCodeUtils）
    /// </summary>
    public static class CompressHelper
    {
        /// <summary>
        /// 解压压缩包（异步）- 兼容多层文件夹，保留层级结构
        /// </summary>
        public static async Task UnzipFileAsync(string zipPath, string unzipDir, string batchId, IProgress<ProcessProgress> progress)
        {
            if (!Directory.Exists(unzipDir)) Directory.CreateDirectory(unzipDir);

            await Task.Run(() =>
            {
                using (var archive = ArchiveFactory.Open(zipPath))
                {
                    // 统计所有非文件夹条目（包括子文件夹内的文件）
                    var totalEntries = archive.Entries.Count(e => !e.IsDirectory);
                    var processed = 0;

                    foreach (var entry in archive.Entries)
                    {
                        if (entry.IsDirectory) continue; // 跳过文件夹条目（SharpCompress会自动创建文件夹）
                        try
                        {
                            // 关键：ExtractFullPath = true 保留压缩包内的完整路径（多层文件夹）
                            entry.WriteToDirectory(unzipDir, new ExtractionOptions
                            {
                                ExtractFullPath = true, // 核心配置，保留层级
                                Overwrite = true
                            });
                            processed++;
                            progress.Report(new ProcessProgress
                            {
                                BatchId = batchId,
                                Type = "Unzip",
                                Progress = (int)((processed / (double)totalEntries) * 100),
                                Message = $"正在解压：{entry.Key}" // entry.Key 是压缩包内的完整路径（含文件夹）
                            });
                        }
                        catch (Exception ex)
                        {
                            progress.Report(new ProcessProgress
                            {
                                BatchId = batchId,
                                Type = "Unzip",
                                Progress = 0,
                                Message = $"解压失败 {entry.Key}：{ex.Message}"
                            });
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 过滤有效图片（兼容多层文件夹，递归扫描所有子目录）
        /// </summary>
        public static List<string> FilterValidImages(string dir, string batchId, IProgress<ProcessProgress> progress)
        {
            var supportFormats = ConfigUtils.AppSetting.GetValue("FileSupportFormats");
            // 关键：SearchOption.AllDirectories 递归扫描所有子文件夹
            var files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
            var validImages = new List<string>();
            var fileHashes = new Dictionary<string, string>();
            var total = files.Length;
            var processed = 0;

            foreach (var file in files)
            {
                processed++;
                try
                {
                    // 0字节过滤
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.Length == 0)
                    {
                        progress.Report(new ProcessProgress
                        {
                            BatchId = batchId,
                            Type = "Filter",
                            Progress = (int)((processed / (double)total) * 100),
                            Message = $"过滤0字节文件：{file}"
                        });
                        File.Delete(file);
                        continue;
                    }

                    // 格式过滤
                    var ext = Path.GetExtension(file).TrimStart('.').ToLower();
                    if (!supportFormats.Contains(ext))
                    {
                        progress.Report(new ProcessProgress
                        {
                            BatchId = batchId,
                            Type = "Filter",
                            Progress = (int)((processed / (double)total) * 100),
                            Message = $"过滤非支持格式：{file}"
                        });
                        File.Delete(file);
                        continue;
                    }

                    // 损坏图片过滤
                    using (var img = Image.FromFile(file)) { }

                    // 重复文件过滤（MD5）
                    var md5 = GetFileMD5(file);
                    if (fileHashes.ContainsKey(md5))
                    {
                        progress.Report(new ProcessProgress
                        {
                            BatchId = batchId,
                            Type = "Filter",
                            Progress = (int)((processed / (double)total) * 100),
                            Message = $"过滤重复文件：{file}"
                        });
                        File.Delete(file);
                        continue;
                    }
                    fileHashes.Add(md5, file);
                    validImages.Add(file);

                    progress.Report(new ProcessProgress
                    {
                        BatchId = batchId,
                        Type = "Filter",
                        Progress = (int)((processed / (double)total) * 100),
                        Message = $"有效图片：{file}"
                    });
                }
                catch (Exception ex)
                {
                    progress.Report(new ProcessProgress
                    {
                        BatchId = batchId,
                        Type = "Filter",
                        Progress = (int)((processed / (double)total) * 100),
                        Message = $"过滤损坏文件：{file}，原因：{ex.Message}"
                    });
                    File.Delete(file);
                }
            }
            return validImages;
        }

        private static string GetFileMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static void CleanTempFiles(string dir, string batchId, IProgress<ProcessProgress> progress)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true); // true 表示递归删除所有子文件夹和文件
                    progress.Report(new ProcessProgress
                    {
                        BatchId = batchId,
                        Type = "Clean",
                        Progress = 100,
                        Message = $"清理临时目录完成：{dir}"
                    });
                }
            }
            catch (Exception ex)
            {
                progress.Report(new ProcessProgress
                {
                    BatchId = batchId,
                    Type = "Clean",
                    Progress = 0,
                    Message = $"清理临时目录失败：{ex.Message}"
                });
            }
        }

        public static string MoveValidImage(string sourceFile, string targetDir)
        {
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(sourceFile)}";
            var targetPath = Path.Combine(targetDir, fileName);
            File.Move(sourceFile, targetPath);
            return targetPath;
        }
    }

    public class ProcessProgress
    {
        public string BatchId { get; set; }
        public string Type { get; set; }
        public int Progress { get; set; }
        public string Message { get; set; }
    }
}