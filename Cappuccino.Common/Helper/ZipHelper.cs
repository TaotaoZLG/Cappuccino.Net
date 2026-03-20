using System;
using System.IO;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;

namespace Cappuccino.Common.Helpers
{
    /// <summary>
    /// Zip压缩解压帮助类
    /// 支持：批量文件压缩、单个文件压缩、整个目录递归压缩
    /// </summary>
    public static class ZipHelper
    {
        /// <summary>
        /// 批量压缩文件到指定Zip包（返回Zip虚拟路径）
        /// </summary>
        /// <param name="filePaths">待压缩的文件物理路径集合</param>
        /// <param name="zipVirtualDir">Zip包保存的虚拟目录（如~/Uploads/Zip/）</param>
        /// <param name="zipFileName">Zip包名称（含后缀，如CaseFiles.zip）</param>
        /// <returns>Zip包的虚拟路径</returns>
        public static string CompressFilesToZip(string[] filePaths, string zipVirtualDir, string zipFileName)
        {
            // 参数校验
            if (filePaths == null || filePaths.Length == 0)
                throw new ArgumentNullException(nameof(filePaths), "待压缩文件路径不能为空");

            // 转换虚拟路径到物理路径
            string zipPhysicalDir = System.Web.HttpContext.Current.Server.MapPath(zipVirtualDir);
            if (!Directory.Exists(zipPhysicalDir))
            {
                Directory.CreateDirectory(zipPhysicalDir);
            }
            string zipPhysicalPath = Path.Combine(zipPhysicalDir, zipFileName);

            // 若Zip已存在则删除
            if (File.Exists(zipPhysicalPath))
            {
                File.Delete(zipPhysicalPath);
            }

            // 使用SharpCompress创建Zip包
            using (var archive = ZipArchive.Create())
            {
                foreach (var filePath in filePaths)
                {
                    if (File.Exists(filePath))
                    {
                        // 添加文件到Zip，保留原文件名
                        archive.AddEntry(Path.GetFileName(filePath), filePath);
                    }
                }
                // 保存Zip包
                archive.SaveTo(zipPhysicalPath, CompressionType.Deflate);
            }

            // 返回Zip虚拟路径
            return Path.Combine(zipVirtualDir, zipFileName).Replace("\\", "/");
        }

        #region 【新增】通用压缩：支持 单个文件 / 整个目录（递归所有文件）
        /// <summary>
        /// 【通用】压缩文件或目录到Zip（自动识别文件/文件夹，递归包含所有子文件）
        /// </summary>
        /// <param name="sourcePath">源路径（可以是单个文件 或 文件夹路径）</param>
        /// <param name="zipVirtualDir">Zip保存虚拟目录（如~/Uploads/Zip/）</param>
        /// <param name="zipFileName">Zip文件名（含后缀）</param>
        /// <returns>Zip虚拟路径</returns>
        public static string CompressToZip(string sourcePath, string zipVirtualDir, string zipFileName)
        {
            // 参数校验
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath), "源文件/目录路径不能为空");
            if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
                throw new FileNotFoundException("指定的文件或目录不存在", sourcePath);

            // 转换输出目录物理路径
            string zipPhysicalDir = System.Web.HttpContext.Current.Server.MapPath(zipVirtualDir);
            if (!Directory.Exists(zipPhysicalDir))
                Directory.CreateDirectory(zipPhysicalDir);
            string zipPhysicalPath = Path.Combine(zipPhysicalDir, zipFileName);

            // 覆盖已存在的压缩包
            if (File.Exists(zipPhysicalPath))
                File.Delete(zipPhysicalPath);

            using (var archive = ZipArchive.Create())
            {
                // 判断：源是【目录】
                if (Directory.Exists(sourcePath))
                {
                    // 递归获取所有文件（包含子目录）
                    var allFiles = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                    foreach (var file in allFiles)
                    {
                        // 保留 相对目录结构（解压后目录不变）
                        string relativePath = file.Substring(sourcePath.Length + 1);
                        archive.AddEntry(relativePath, file);
                    }
                }
                // 判断：源是【单个文件】
                else if (File.Exists(sourcePath))
                {
                    archive.AddEntry(Path.GetFileName(sourcePath), sourcePath);
                }

                // 保存压缩包
                archive.SaveTo(zipPhysicalPath, CompressionType.Deflate);
            }

            // 返回虚拟路径
            return Path.Combine(zipVirtualDir, zipFileName).Replace("\\", "/");
        }
        #endregion
    }
}