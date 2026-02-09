using System;
using System.IO;
using System.Collections.Generic;
using SharpSevenZip; // SharpSevenZip 核心命名空间
using ICSharpCode.SharpZipLib.Zip;

namespace Cappuccino.Common.Util
{
    /// <summary>
    /// 压缩包解压工具（支持ZIP/RAR/7Z，基于 SharpSevenZip 实现）
    /// </summary>
    public class CompressHelper
    {
        static CompressHelper()
        {
            // 初始化 SharpSevenZip（自动加载内置的7z.dll，无需手动配置）
            SharpSevenZipBase.SetLibraryPath();
        }

        /// <summary>
        /// 解压压缩包到指定目录（统一入口，适配ZIP/RAR/7Z）
        /// </summary>
        /// <param name="compressPath">压缩包路径</param>
        /// <param name="extractDir">解压目录</param>
        /// <returns>解压后的文件列表</returns>
        public static List<string> ExtractCompress(string compressPath, string extractDir)
        {
            // 参数校验
            if (string.IsNullOrWhiteSpace(compressPath))
                throw new ArgumentNullException(nameof(compressPath), "压缩包路径不能为空");
            if (!File.Exists(compressPath))
                throw new FileNotFoundException("压缩包不存在", compressPath);

            // 确保解压目录存在
            Directory.CreateDirectory(extractDir);
            string ext = Path.GetExtension(compressPath).ToLowerInvariant();
            List<string> extractFiles = new List<string>();

            switch (ext)
            {
                case ".zip":
                    // ZIP格式：优先用 SharpZipLib（你项目已引用，保持兼容）
                    extractFiles = ExtractZip(compressPath, extractDir);
                    break;
                case ".rar":
                case ".7z":
                    // RAR/7Z格式：用 SharpSevenZip 实现
                    extractFiles = Extract7zRar(compressPath, extractDir);
                    break;
                default:
                    throw new NotSupportedException($"不支持的压缩格式：{ext}（仅支持ZIP/RAR/7Z）");
            }

            return extractFiles;
        }

        /// <summary>
        /// 解压ZIP（SharpZipLib，保持你项目原有逻辑）
        /// </summary>
        private static List<string> ExtractZip(string zipPath, string extractDir)
        {
            List<string> files = new List<string>();
            using (FileStream fs = File.OpenRead(zipPath))
            using (ZipFile zf = new ZipFile(fs))
            {
                foreach (ZipEntry entry in zf)
                {
                    if (entry.IsDirectory || string.IsNullOrWhiteSpace(entry.Name))
                        continue;

                    string filePath = Path.Combine(extractDir, entry.Name);
                    string fileDir = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrWhiteSpace(fileDir))
                        Directory.CreateDirectory(fileDir);

                    // 写入解压后的文件
                    using (Stream stream = zf.GetInputStream(entry))
                    using (FileStream fsOut = File.Create(filePath))
                    {
                        stream.CopyTo(fsOut);
                        files.Add(filePath);
                    }
                }
            }
            return files;
        }

        /// <summary>
        /// 解压7Z/RAR（基于 SharpSevenZip 实现，核心适配）
        /// </summary>
        private static List<string> Extract7zRar(string compressPath, string extractDir)
        {
            List<string> files = new List<string>();

            // 1. 创建 SharpSevenZip 的解压器（自动识别压缩格式）
            using (SharpSevenZipExtractor extractor = new SharpSevenZipExtractor(compressPath))
            {
                // 校验压缩包是否有效（过滤损坏压缩包）
                if (!extractor.Check())
                    throw new InvalidDataException($"压缩包已损坏：{compressPath}");

                // 2. 遍历压缩包内的文件，逐个解压
                foreach (ArchiveFileInfo fileInfo in extractor.ArchiveFileData)
                {
                    // 跳过目录、空文件名
                    if (fileInfo.IsDirectory || string.IsNullOrWhiteSpace(fileInfo.FileName))
                        continue;

                    // 拼接解压后的文件路径
                    string targetFilePath = Path.Combine(extractDir, fileInfo.FileName);
                    string targetFileDir = Path.GetDirectoryName(targetFilePath);
                    if (!string.IsNullOrWhiteSpace(targetFileDir))
                        Directory.CreateDirectory(targetFileDir);

                    try
                    {
                        // 3. 解压单个文件（SharpSevenZip 稳定支持）
                        extractor.ExtractFile(fileInfo.FileName, targetFilePath);

                        // 4. 过滤0字节文件（提前清理无效文件）
                        FileInfo file = new FileInfo(targetFilePath);
                        if (file.Length == 0)
                        {
                            File.Delete(targetFilePath);
                            continue;
                        }

                        files.Add(targetFilePath);
                    }
                    catch (Exception)
                    {
                        // 过滤损坏的压缩包内文件，不中断整体解压
                        if (File.Exists(targetFilePath))
                            File.Delete(targetFilePath);
                        continue;
                    }
                }
            }

            return files;
        }
    }
}