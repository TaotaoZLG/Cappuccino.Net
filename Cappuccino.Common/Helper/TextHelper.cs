using System;
using System.Linq;
using Cappuccino.Common.Extensions;

namespace Cappuccino.Common.Helper
{
    public class TextHelper
    {
        /// <summary>
        /// 格式化特殊字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceStr(string str)
        {
            return str.Replace("\\", "")
                .Replace("/", "")
                .Replace(":", "")
                .Replace("*", "")
                .Replace("?", "")
                .Replace("\"", "")
                .Replace("<", "")
                .Replace(">", "")
                .Replace("|", "");
        }

        /// <summary>
        /// 获取默认值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetCustomValue(string value, string defaultValue)
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// 截取指定长度的字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetSubString(string value, int length, bool ellipsis = false)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            if (value.Length > length)
            {
                value = value.Substring(0, length);
                if (ellipsis)
                {
                    value += "...";
                }
            }
            return value;
        }

        public static string GetCustomSubString(string value, int startlength = 0, int endlength = 0, bool ellipsis = false)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            if (value.Length > endlength)
            {
                value = value.Substring(startlength, endlength);
                if (ellipsis)
                {
                    value += "...";
                }
            }
            return value;
        }

        /// <summary>
        /// 字符串转指定类型数组
        /// </summary>
        /// <param name="value"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static T[] SplitToArray<T>(string value, char split)
        {
            T[] arr = value.Split(new string[] { split.ToString() }, StringSplitOptions.RemoveEmptyEntries).CastSuper<T>().ToArray();
            return arr;
        }

        /// <summary>
        /// 从参数字符串中提取指定参数的值（支持动态分隔符）
        /// </summary>
        /// <param name="paramString">参数字符串（格式：key1=value1{separator}key2=value2...）</param>
        /// <param name="paramKey">要提取的参数名</param>
        /// <param name="separator">参数之间的分隔符（默认：&）</param>
        /// <returns>参数值（若不存在则返回null）</returns>
        public static string ExtractParamValue(string paramString, string paramKey, char separator = '&')
        {
            if (string.IsNullOrEmpty(paramString) || string.IsNullOrEmpty(paramKey))
                return null;

            // 按动态传入的分隔符分割所有键值对
            string[] keyValuePairs = paramString.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string pair in keyValuePairs)
            {
                // 按=分割键和值（仅以第一个=作为分割点）
                int equalIndex = pair.IndexOf('=');
                if (equalIndex <= 0) // 没有=或键为空，跳过
                    continue;

                string key = pair.Substring(0, equalIndex).Trim();
                string value = equalIndex < pair.Length - 1
                    ? pair.Substring(equalIndex + 1).Trim()
                    : string.Empty; // 处理key=的情况

                // 匹配目标参数名（不区分大小写）
                if (string.Equals(key, paramKey, StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }

            return null; // 未找到参数
        }
    }
}
