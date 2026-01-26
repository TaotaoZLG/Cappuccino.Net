using System;
using Cappuccino.Common.Extensions;

namespace Cappuccino.Common.Helper
{
    /// <summary>
    /// 时间帮助类
    /// </summary>
    public class StartEndDateHelper
    {
        /// <summary>
        /// 获取开始时间（支持yyyy-MM-dd和yyyy-MM-dd HH:mm:ss格式）
        /// 纯日期默认00:00:00，带时间则保留原时间
        /// </summary>
        public static DateTime GteStartDate(string startEndDate)
        {
            if (!string.IsNullOrEmpty(startEndDate) && startEndDate != " ~ ")
            {
                if (startEndDate.Contains("~"))
                {
                    // 移除可能的"+"字符
                    startEndDate = startEndDate.Replace("+", "");
                    var dts = startEndDate.Split('~');
                    string startDateStr = dts[0].Trim();

                    DateTime startDt = startDateStr.ParseToDateTime(DateTime.MinValue);

                    return startDt;
                }
            }
            return DateTime.MinValue;
        }

        /// <summary>
        /// 获取结束时间（支持yyyy-MM-dd和yyyy-MM-dd HH:mm:ss格式）
        /// 纯日期默认23:59:59，带时间则保留原时间
        /// </summary>
        public static DateTime GteEndDate(string startEndDate)
        {
            if (!string.IsNullOrEmpty(startEndDate) && startEndDate != " ~ ")
            {
                if (startEndDate.Contains("~"))
                {
                    // 移除可能的"+"字符
                    startEndDate = startEndDate.Replace("+", "");
                    var dts = startEndDate.Split('~');
                    string endDateStr = dts[1].Trim();

                    DateTime endDt = endDateStr.ParseToDateTime(DateTime.MinValue);

                    if (endDt != DateTime.MinValue)
                    {
                        // 检查原字符串是否仅为日期（不含时间部分）
                        bool isDateOnly = !endDateStr.Contains(" ") && (endDateStr.Length == 10 || endDateStr.Contains("/"));

                        if (isDateOnly)
                        {
                            return endDt.Date.AddDays(1).AddSeconds(-1);
                        }
                        else
                        {
                            return endDt;
                        }
                    }
                }
            }
            return DateTime.MinValue;
        }
    }
}

