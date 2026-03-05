using System;
using System.Globalization;

namespace Cappuccino.Common.Helper
{
    public class DateTimeHelper
    {
        #region 当前时间


        /// <summary>
        /// 返回当前时间的标准日期格式
        /// </summary>
        /// <returns>yyyy-MM-dd</returns>
        public static string GetDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
        /// <summary>
        /// 返回当前时间的标准时间格式string
        /// </summary>
        /// <returns>HH:mm:ss</returns>
        public static string GetTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }
        /// <summary>
        /// 返回当前时间的标准时间格式string
        /// </summary>
        /// <returns>yyyy-MM-dd HH:mm:ss</returns>
        public static string GetDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        /// <summary>
        /// 返回当前时间的标准时间格式
        /// </summary>
        /// <returns>yyyy-MM-dd HH:mm:ss:fffffff</returns>
        public static string GetDateTimeF()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fffffff");
        }
        #endregion

        #region 毫秒转天时分秒
        /// <summary>
        /// 毫秒转天时分秒
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static string FormatTime(long ms)
        {
            int ss = 1000;
            int mi = ss * 60;
            int hh = mi * 60;
            int dd = hh * 24;

            long day = ms / dd;
            long hour = (ms - day * dd) / hh;
            long minute = (ms - day * dd - hour * hh) / mi;
            long second = (ms - day * dd - hour * hh - minute * mi) / ss;
            long milliSecond = ms - day * dd - hour * hh - minute * mi - second * ss;

            string sDay = day < 10 ? "0" + day : "" + day; //天
            string sHour = hour < 10 ? "0" + hour : "" + hour;//小时
            string sMinute = minute < 10 ? "0" + minute : "" + minute;//分钟
            string sSecond = second < 10 ? "0" + second : "" + second;//秒
            string sMilliSecond = milliSecond < 10 ? "0" + milliSecond : "" + milliSecond;//毫秒
            sMilliSecond = milliSecond < 100 ? "0" + sMilliSecond : "" + sMilliSecond;

            return string.Format("{0} 天 {1} 小时 {2} 分 {3} 秒", sDay, sHour, sMinute, sSecond);
        }
        #endregion

        #region 获取unix时间戳
        /// <summary>
        /// 将DateTime对象转换为Unix时间戳（毫秒）
        /// </summary>
        /// <param name="dt">时间</param>
        /// <returns></returns>
        public static long GetUnixTimeStamp(DateTime dt)
        {
            long unixTime = ((DateTimeOffset)dt).ToUnixTimeMilliseconds();
            return unixTime;
        }

        /// <summary>
        /// 获取当前时间的Unix时间戳（毫秒）
        /// </summary>
        /// <returns>当前时间的Unix时间戳（毫秒）</returns>
        public static long GetCurrentUnixTimestamp()
        {
            return GetUnixTimeStamp(DateTime.UtcNow);
        }

        /// <summary>
        /// 指定时间戳转为时间。
        /// </summary>
        /// <param name="timeStamp">需要被反转的时间戳</param>
        /// <param name="accurateToMilliseconds">是否精确到毫秒</param>
        /// <returns>返回时间戳对应的DateTime</returns>
        public static DateTime GetTime(long timeStamp, bool accurateToMilliseconds = false)
        {
            if (accurateToMilliseconds)
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(timeStamp).LocalDateTime;
            }
            else
            {
                return DateTimeOffset.FromUnixTimeSeconds(timeStamp).LocalDateTime;
            }
        }

        /// <summary>
        /// 取当前时间的时间戳，高并发情况下会有重复。想要解决这问题请使用加锁或其他方式。
        /// </summary>
        /// <param name="accurateToMilliseconds">是否精确到毫秒</param>
        /// <returns>返回long类型时间戳</returns>
        public static long GetTimeStamp(bool accurateToMilliseconds = false)
        {
            if (accurateToMilliseconds)
            {
                return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
            else
            {
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }

        #endregion

        #region 获取日期天的最小时间
        public static DateTime GetDayMinDate(DateTime dt)
        {
            DateTime min = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
            return min;
        }
        #endregion

        #region 获取日期天的最大时间
        public static DateTime GetDayMaxDate(DateTime dt)
        {
            DateTime max = new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59);
            return max;
        }
        #endregion

        #region 获取日期天的最大时间
        public static string FormatDateTime(DateTime? dt)
        {
            if (dt != null)
            {
                if (dt.Value.Year == DateTime.Now.Year)
                {
                    return dt.Value.ToString("MM-dd HH:mm");
                }
                else
                {
                    return dt.Value.ToString("yyyy-MM-dd HH:mm");
                }
            }
            return string.Empty;
        }
        #endregion

        #region 获得两个日期的间隔天数
        public static int DateDiff(DateTime dateStart, DateTime dateEnd)
        {
            DateTime start = Convert.ToDateTime(dateStart.ToShortDateString());
            DateTime end = Convert.ToDateTime(dateEnd.ToShortDateString());
            TimeSpan sp = end.Subtract(start);     
            //返回绝对值 zlg
            return Math.Abs(sp.Days);
        }
        #endregion

        #region 格式化日期时间
        /// <summary>
        /// 格式化日期时间
        /// </summary>
        /// <param name="dateTime1">日期时间</param>
        /// <param name="dateMode">显示模式</param>
        /// <returns>0-9种模式的日期</returns>
        public static string FormatDate(DateTime dateTime1, string dateMode)
        {
            switch (dateMode)
            {
                case "0":
                    return dateTime1.ToString("yyyy-MM-dd");
                case "1":
                    return dateTime1.ToString("yyyy-MM-dd HH:mm:ss");
                case "2":
                    return dateTime1.ToString("yyyy/MM/dd");
                case "3":
                    return dateTime1.ToString("yyyy年MM月dd日");
                case "4":
                    return dateTime1.ToString("MM-dd");
                case "5":
                    return dateTime1.ToString("MM/dd");
                case "6":
                    return dateTime1.ToString("MM月dd日");
                case "7":
                    return dateTime1.ToString("yyyy-MM");
                case "8":
                    return dateTime1.ToString("yyyy/MM");
                case "9":
                    return dateTime1.ToString("yyyy年MM月");
                default:
                    return dateTime1.ToString();
            }
        }

        /// <summary>
        /// 返回标准时间
        /// </summary>
        /// <param name="fDateTime">转换时间</param>
        /// <param name="formatStr">转换格式</param>
        /// <returns>转换后的时间</returns>
        public static string GetStandardDateTime(string fDateTime, string formatStr)
        {
            if (fDateTime == "0000-0-0 0:00:00") return fDateTime;
            DateTime time = new DateTime(1900, 1, 1, 0, 0, 0, 0);
            if (DateTime.TryParse(fDateTime, out time))
            {
                return time.ToString(formatStr);
            }
            else
            {
                return "N/A";
            }
        }

        /// <summary>
        /// 返回标准时间 yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <param name="fDateTime">转换时间</param>
        /// <returns>yyyy-MM-dd HH:mm:ss</returns>
        public static string GetStandardDateTime(string fDateTime)
        {
            return GetStandardDateTime(fDateTime, "yyyy-MM-dd HH:mm:ss");
        }
        /// <summary>
        /// 返回标准时间 yyyy-MM-dd
        /// </summary>
        /// <param name="fDate">转换时间</param>
        /// <returns>yyyy-MM-dd</returns>
        public static string GetStandardDate(string fDate)
        {
            return GetStandardDateTime(fDate, "yyyy-MM-dd");
        }
        #endregion        

        #region 自定义输入和输出日期时间，返回格式化后的日期时间字符串
        /// <summary>
        /// 接受输入的日期时间字符串和输出的日期时间格式作为参数，并返回格式化后的日期时间字符串
        /// </summary>
        /// <param name="dateTimeStr">输入的日期时间字符串</param>
        /// <param name="dateTimeFormat">输入的日期时间格式</param>
        /// <param name="outputFormat">输出的日期时间格式</param>
        /// <returns></returns>
        public static string ConvertDateTimeParseRxact(string dateTimeStr, string dateTimeFormat, string outputFormat)
        {
            DateTime dateTime = DateTime.ParseExact(dateTimeStr, dateTimeFormat, CultureInfo.InvariantCulture);
            string formattedDateTimeStr = dateTime.ToString(outputFormat);
            return formattedDateTimeStr;
        }
        #endregion
    }
}
