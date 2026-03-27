using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Common.Extensions
{
    /// <summary>
    /// Path扩展方法：获取相对路径（虚拟路径）
    /// </summary>
    public static class PathExtensions
    {
        public static string GetRelativePath(string relativeTo, string path)
        {
            if (string.IsNullOrEmpty(relativeTo) || string.IsNullOrEmpty(path))
            {
                return path;
            }

            // 统一路径分隔符
            relativeTo = Path.GetFullPath(relativeTo).TrimEnd('\\', '/');
            path = Path.GetFullPath(path).TrimEnd('\\', '/');

            // 如果路径相同
            if (string.Equals(relativeTo, path, StringComparison.OrdinalIgnoreCase))
            {
                return ".";
            }

            // 如果不在同一驱动器
            if (!string.Equals(Path.GetPathRoot(relativeTo), Path.GetPathRoot(path), StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            // 分割路径
            string[] relativeToParts = relativeTo.Split('\\', '/');
            string[] pathParts = path.Split('\\', '/');

            // 查找共同前缀
            int commonLength = 0;
            int minLen = Math.Min(relativeToParts.Length, pathParts.Length);
            for (int i = 0; i < minLen; i++)
            {
                if (string.Equals(relativeToParts[i], pathParts[i], StringComparison.OrdinalIgnoreCase))
                {
                    commonLength++;
                }
                else
                {
                    break;
                }
            }

            // 构建相对路径
            var result = new List<string>();

            // 添加 ".." 表示向上目录
            for (int i = commonLength; i < relativeToParts.Length; i++)
            {
                result.Add("..");
            }

            // 添加剩余路径
            for (int i = commonLength; i < pathParts.Length; i++)
            {
                result.Add(pathParts[i]);
            }

            return result.Count == 0 ? "." : string.Join("\\", result);
        }
    }
}