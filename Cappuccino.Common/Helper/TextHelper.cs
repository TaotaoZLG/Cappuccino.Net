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
    }
}
