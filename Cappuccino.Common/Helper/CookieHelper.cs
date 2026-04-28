using System;
using System.Net;
using System.Web;

namespace Cappuccino.Common.Helper
{
    public class CookieHelper
    {
        #region Cookie操作
        /// <summary>
        /// 写cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="strValue">值</param>
        public static void Set(string strName, string strValue)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[strName];
            if (cookie == null)
            {
                cookie = new HttpCookie(strName);
            }
            cookie.Value = strValue;
            // 设置到站点根路径，保证所有路径都能携带此 Cookie
            cookie.Path = "/";
            HttpContext.Current.Response.AppendCookie(cookie);
        }
        /// <summary>
        /// 写cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="strValue">值</param>
        /// <param name="strValue">过期时间(分钟)</param>
        public static void Set(string strName, string strValue, int expires)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[strName];
            if (cookie == null)
            {
                cookie = new HttpCookie(strName);
            }
            cookie.Value = strValue;
            cookie.Expires = DateTime.Now.AddMinutes(expires);
            // 设置到站点根路径，保证所有路径都能携带此 Cookie
            cookie.Path = "/";
            HttpContext.Current.Response.AppendCookie(cookie);
        }
        /// <summary>
        /// 写cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <param name="strValue">值</param>
        /// <param name="expires">过期时间(时间间隔)</param>
        public static void Set(string strName, string strValue, TimeSpan expires)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[strName];
            if (cookie == null)
            {
                cookie = new HttpCookie(strName);
            }
            cookie.Value = strValue;
            cookie.Expires = DateTime.Now.Add(expires);
            // 设置到站点根路径，保证所有路径都能携带此 Cookie
            cookie.Path = "/";
            HttpContext.Current.Response.AppendCookie(cookie);
        }
        /// <summary>
        /// 读cookie值
        /// </summary>
        /// <param name="strName">名称</param>
        /// <returns>cookie值</returns>
        public static string Get(string strName)
        {
            if (HttpContext.Current.Request.Cookies != null && HttpContext.Current.Request.Cookies[strName] != null)
            {
                return HttpContext.Current.Request.Cookies[strName].Value.ToString();
            }
            return "";
        }
        /// <summary>
        /// 删除Cookie对象
        /// </summary>
        /// <param name="CookiesName">Cookie对象名称</param>
        public static void Remove(string CookiesName)
        {
            HttpCookie cookie = new HttpCookie(CookiesName.Trim());
            cookie.Expires = DateTime.Now.AddYears(-5);
            // 设置到站点根路径，保证所有路径都能携带此 Cookie
            cookie.Path = "/";
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
        #endregion
    }
}
