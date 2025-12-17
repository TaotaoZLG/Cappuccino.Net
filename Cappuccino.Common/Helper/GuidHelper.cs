using System;

namespace Cappuccino.Common.Helper
{
    public static class GuidHelper
    {
        /// <summary>
        /// 获取Guid
        /// </summary>
        /// <param name="replaceDash">是否不包含连接字符'-'</param>
        /// <returns></returns>
        public static string GetGuid(bool replaceDash = false)
        {
            string guid = Guid.NewGuid().ToString();
            if (replaceDash)
            {
                guid = guid.Replace("-", string.Empty);
            }
            return guid;
        }

        /// <summary>
        /// 42位的Guid+时间戳
        /// </summary>
        /// <param name="replaceDash">是否不包含连接字符'-'</param>
        /// <returns></returns>
        public static string GetTimestampGuid(bool replaceDash = false)
        {
            string guid = Guid.NewGuid().ToString();
            if (replaceDash)
            {
                guid = guid.Replace("-", string.Empty);
            }
            long timestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds / 1000;
            return $"{guid}{timestamp}";
        }

        /// <summary>
        /// 转为有序的GUID，长度为50字符
        /// </summary>
        /// <param name="Guid">新的GUID</param>
        /// <returns></returns>
        public static string GetToSequentialGuid(Guid guid)
        {
            DateTime cstTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "China Standard Time");
            long ticks = cstTime.Ticks;
            var timeStr = (ticks / 10000).ToString("x8");
            var newGuid = $"{timeStr.PadLeft(13, '0')}-{guid}";
            return newGuid;
        }
    }
}
