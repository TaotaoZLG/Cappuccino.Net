using System.Web;
using Cappuccino.Common.Extensions;

namespace Cappuccino.Common.Helper
{
    public class SessionHelper
    {
        #region Session操作

        /// <summary>
        /// 设置Session全局超时时间
        /// </summary>
        /// <param name="timeout">超时时间(分钟)，最大为1440(24小时)</param>
        public static void SetTimeout(int timeout)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Timeout = timeout;
            }
        }

        /// <summary>
        /// 写Session
        /// </summary>
        /// <typeparam name="T">Session键值的类型</typeparam>
        /// <param name="key">Session的键名</param>
        /// <param name="value">Session的键值</param>
        public static void Set<T>(string key, T value)
        {
            if (key.IsEmpty())
                return;
            HttpContext.Current.Session[key] = value;
        }

        /// <summary>
        /// 写Session
        /// </summary>
        /// <param name="key">Session的键名</param>
        /// <param name="value">Session的键值</param>
        public static void Set(string key, string value)
        {
            Set<string>(key, value);
        }

        /// <summary>
        /// 读取Session的值
        /// </summary>
        /// <param name="key">Session的键名</param>        
        public static string Get(string key)
        {
            if (key.IsEmpty())
                return string.Empty;
            return HttpContext.Current.Session[key] as string;
        }

        /// <summary>
        /// 读取Session的值
        /// </summary>
        /// <param name="key">Session的键名</param>        
        public static T Get<T>(string key)
        {
            if (key.IsEmpty())
                return default;
            return (T)HttpContext.Current.Session[key];
        }

        /// <summary>
        /// 删除指定Session
        /// </summary>
        /// <param name="key">Session的键名</param>
        public static void Remove(string key)
        {
            if (key.IsEmpty())
                return;
            HttpContext.Current.Session.Contents.Remove(key);
        }

        #endregion
    }
}
