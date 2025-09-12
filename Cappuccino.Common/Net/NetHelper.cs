using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Web;
using Cappuccino.Common.Extensions;

namespace Cappuccino.Common.Net
{
    public class NetHelper
    {
        #region Ip(获取Ip)
        /// <summary>
        /// 获取Ip
        /// </summary>
        public static string GetIp
        {
            get
            {
                var result = string.Empty;
                if (HttpContext.Current != null)
                    result = GetWebClientIp();
                if (result.IsEmpty())
                    result = GetLanIp();
                return result;
            }
        }

        /// <summary>
        /// 获取Web客户端的Ip
        /// </summary>
        private static string GetWebClientIp()
        {
            var ip = GetWebRemoteIp();
            foreach (var hostAddress in Dns.GetHostAddresses(ip))
            {
                if (hostAddress.AddressFamily == AddressFamily.InterNetwork)
                    return hostAddress.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取Web远程Ip
        /// </summary>
        private static string GetWebRemoteIp()
        {
            return HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }

        /// <summary>
        /// 获取局域网IP
        /// </summary>
        private static string GetLanIp()
        {
            foreach (var hostAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (hostAddress.AddressFamily == AddressFamily.InterNetwork)
                    return hostAddress.ToString();
            }
            return string.Empty;
        }

        #endregion

        #region Host(获取主机名)

        /// <summary>
        /// 获取主机名
        /// </summary>
        public static string Host
        {
            get
            {
                return HttpContext.Current == null ? Dns.GetHostName() : GetWebClientHostName();
            }
        }

        /// <summary>
        /// 获取Web客户端主机名
        /// </summary>
        private static string GetWebClientHostName()
        {
            if (!HttpContext.Current.Request.IsLocal)
                return string.Empty;
            var ip = GetWebRemoteIp();
            var result = Dns.GetHostEntry(IPAddress.Parse(ip)).HostName;
            if (result == "localhost.localdomain")
                result = Dns.GetHostName();
            return result;
        }

        #endregion

        #region 获取mac地址
        /// <summary>
        /// 返回描述本地计算机上的网络接口的对象(网络接口也称为网络适配器)。
        /// </summary>
        /// <returns></returns>
        public static NetworkInterface[] NetCardInfo()
        {
            return NetworkInterface.GetAllNetworkInterfaces();
        }
        ///<summary>
        /// 通过NetworkInterface读取网卡Mac
        ///</summary>
        ///<returns></returns>
        public static List<string> GetMacByNetworkInterface()
        {
            List<string> macs = new List<string>();
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in interfaces)
            {
                macs.Add(ni.GetPhysicalAddress().ToString());
            }
            return macs;
        }
        #endregion

        #region Ip城市(获取Ip城市)
        /// <summary>
        /// 获取IP地址信息
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string GetIpLocation(string ip)
        {
            string res = "";
            try
            {
                string url = "http://apis.juhe.cn/ip/ip2addr?ip=" + ip + "&dtype=json&key=b39857e36bee7a305d55cdb113a9d725";
                res = HttpMethods.HttpGet(url);
                var resjson = res.ToObject<objex>();
                if (resjson.resultcode == "200")
                {
                    res = resjson.result.area + " " + resjson.result.location;
                }
                else
                {
                    res = "";
                }
            }
            catch
            {
                res = "";
            }
            if (!string.IsNullOrEmpty(res))
            {
                return res;
            }
            try
            {
                string url = "https://sp0.baidu.com/8aQDcjqpAAV3otqbppnN2DJv/api.php?query=" + ip + "&resource_id=6006&ie=utf8&oe=gbk&format=json";
                res = HttpMethods.HttpGet(url, Encoding.GetEncoding("GBK"));
                var resjson = res.ToObject<obj>();
                res = resjson.data[0].location;
            }
            catch
            {
                res = "";
            }
            return res;
        }
        /// <summary>
        /// 百度接口
        /// </summary>
        public class obj
        {
            public List<dataone> data { get; set; }
        }
        public class dataone
        {
            public string location { get; set; }
        }
        /// <summary>
        /// 聚合数据
        /// </summary>
        public class objex
        {
            public string resultcode { get; set; }
            public dataoneex result { get; set; }
            public string reason { get; set; }
            public string error_code { get; set; }
        }
        public class dataoneex
        {
            public string area { get; set; }
            public string location { get; set; }
        }
        #endregion

        #region Browser(获取浏览器信息)
        /// <summary>
        /// 获取浏览器信息
        /// </summary>
        public static string Browser
        {
            get
            {
                if (HttpContext.Current == null)
                    return string.Empty;
                var browser = HttpContext.Current.Request.Browser;
                return string.Format("{0} {1}", browser.Browser, browser.Version);
            }
        }
        #endregion


        /// <summary>
        /// 获取POST请求体内容（支持表单、JSON等格式）
        /// </summary>
        /// <param name="request">HTTP请求对象</param>
        /// <returns>请求体内容（超长会截断）</returns>
        public static string GetRequestBody(HttpRequestBase request)
        {
            // 非POST请求直接返回空
            if (request.HttpMethod?.ToUpper() != "POST")
                return string.Empty;

            try
            {
                // 检查请求流是否可读取
                if (!request.InputStream.CanRead)
                    return "请求流不可读取";

                // 记录原始流位置（后续需要恢复，避免影响其他组件读取）
                long originalPosition = request.InputStream.Position;

                // 重置流位置到起始点（防止之前的读取导致数据丢失）
                request.InputStream.Position = 0;

                // 读取流内容（使用using确保释放资源）
                using (var reader = new System.IO.StreamReader(
                    request.InputStream,
                    encoding: System.Text.Encoding.UTF8,  // 明确编码，避免默认编码问题
                    detectEncodingFromByteOrderMarks: true,
                    bufferSize: 4096,
                    leaveOpen: true  // 保持流打开，其他组件可能需要继续读取
                ))
                {
                    string requestBody = reader.ReadToEnd();

                    // 恢复流位置到原始位置
                    request.InputStream.Position = originalPosition;

                    // 限制日志长度（避免大文件/大JSON导致日志表过大）
                    const int maxLength = 2000;
                    if (!string.IsNullOrEmpty(requestBody) && requestBody.Length > maxLength)
                    {
                        return requestBody.Substring(0, maxLength) + "...[内容过长，已截断]";
                    }

                    // 处理空请求体（如表单提交但无数据）
                    return string.IsNullOrEmpty(requestBody) ? "无POST数据" : requestBody;
                }
            }
            catch (Exception ex)
            {
                // 记录读取失败原因，但不影响主业务
                return $"请求体读取失败：{ex.Message.Substring(0, 100)}"; // 限制错误信息长度
            }
        }

        /// <summary>
        /// 从User-Agent解析操作系统信息
        /// </summary>
        /// <param name="userAgent">请求头中的User-Agent字符串</param>
        /// <returns>操作系统名称（如Windows 10、macOS、Android等）</returns>
        public static string GetSystemOs(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return "未知操作系统";

            userAgent = userAgent.ToLower();

            // Windows系统
            if (userAgent.Contains("windows"))
            {
                if (userAgent.Contains("windows nt 10.0"))
                    return "Windows 10/11"; // Win11的UA与Win10一致，暂合并显示
                if (userAgent.Contains("windows nt 6.3"))
                    return "Windows 8.1";
                if (userAgent.Contains("windows nt 6.2"))
                    return "Windows 8";
                if (userAgent.Contains("windows nt 6.1"))
                    return "Windows 7";
                if (userAgent.Contains("windows nt 6.0"))
                    return "Windows Vista";
                if (userAgent.Contains("windows nt 5.1"))
                    return "Windows XP";
                return "Windows（未知版本）";
            }

            // 苹果系统
            if (userAgent.Contains("mac os x"))
            {
                if (userAgent.Contains("10_15"))
                    return "macOS Catalina";
                if (userAgent.Contains("11_"))
                    return "macOS Big Sur";
                if (userAgent.Contains("12_"))
                    return "macOS Monterey";
                if (userAgent.Contains("13_"))
                    return "macOS Ventura";
                return "macOS（未知版本）";
            }

            // 移动设备系统
            if (userAgent.Contains("iphone") || userAgent.Contains("ipad"))
                return "iOS";
            if (userAgent.Contains("android"))
                return "Android";
            if (userAgent.Contains("harmonyos"))
                return "HarmonyOS（鸿蒙）";

            // 其他系统
            if (userAgent.Contains("linux"))
                return "Linux";
            if (userAgent.Contains("unix"))
                return "Unix";
            if (userAgent.Contains("chromeos"))
                return "Chrome OS";

            return "未知操作系统";
        }

        /// <summary>
        /// 从User-Agent解析浏览器信息
        /// </summary>
        /// <param name="userAgent">请求头中的User-Agent字符串</param>
        /// <returns>浏览器名称（如Chrome、Firefox、Edge等）</returns>
        public static string GetBrowser(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return "未知浏览器";

            userAgent = userAgent.ToLower();

            // 基于Chromium的浏览器（优先判断特殊标识）
            if (userAgent.Contains("edg"))
                return "Microsoft Edge";
            if (userAgent.Contains("chrome") && !userAgent.Contains("edg"))
            {
                if (userAgent.Contains("360se"))
                    return "360安全浏览器";
                if (userAgent.Contains("qqbrowser"))
                    return "QQ浏览器";
                if (userAgent.Contains("sougou"))
                    return "搜狗浏览器";
                return "Google Chrome";
            }

            // 其他常见浏览器
            if (userAgent.Contains("firefox"))
                return "Mozilla Firefox";
            if (userAgent.Contains("safari") && !userAgent.Contains("chrome"))
                return "Safari";
            if (userAgent.Contains("opera"))
                return "Opera";
            if (userAgent.Contains("msie") || userAgent.Contains("trident"))
                return "Internet Explorer";

            // 移动端浏览器
            if (userAgent.Contains("micromessenger"))
                return "微信浏览器";
            if (userAgent.Contains("alipay"))
                return "支付宝浏览器";
            if (userAgent.Contains("ucbrowser"))
                return "UC浏览器";

            return "未知浏览器";
        }

        /// <summary>
        /// 获取请求中的所有参数（查询字符串、表单数据、路由参数）
        /// </summary>
        /// <param name="request">HTTP请求对象</param>
        /// <returns>格式化的参数字符串（key=value&key=value）</returns>
        public static string GetRequestParams(HttpRequestBase request)
        {
            if (request == null)
                return string.Empty;

            try
            {
                // 存储所有参数的集合
                var paramList = new List<string>();

                // 1. 添加查询字符串参数（GET参数）
                if (request.QueryString != null && request.QueryString.AllKeys != null)
                {
                    foreach (var key in request.QueryString.AllKeys)
                    {
                        if (!string.IsNullOrEmpty(key))
                        {
                            // 解码参数值（处理中文/特殊字符）
                            string value = HttpUtility.UrlDecode(request.QueryString[key] ?? string.Empty);
                            paramList.Add($"{key}={value}");
                        }
                    }
                }

                // 2. 添加表单参数（POST表单数据）
                if (request.Form != null && request.Form.AllKeys != null)
                {
                    foreach (var key in request.Form.AllKeys)
                    {
                        if (!string.IsNullOrEmpty(key))
                        {
                            // 敏感参数（如密码）可在此处过滤
                            if (key.Equals("password", StringComparison.OrdinalIgnoreCase) ||
                                key.Equals("pwd", StringComparison.OrdinalIgnoreCase))
                            {
                                paramList.Add($"{key}=***"); // 密码脱敏
                            }
                            else
                            {
                                string value = HttpUtility.HtmlEncode(request.Form[key] ?? string.Empty);
                                paramList.Add($"{key}={value}");
                            }
                        }
                    }
                }

                // 3. 添加路由参数（如 /User/Edit/1 中的id=1）
                if (request.RequestContext != null && request.RequestContext.RouteData.Values != null)
                {
                    foreach (var item in request.RequestContext.RouteData.Values)
                    {
                        // 排除控制器和动作名（框架自带参数）
                        if (item.Key.Equals("controller", StringComparison.OrdinalIgnoreCase) ||
                            item.Key.Equals("action", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (item.Value != null)
                        {
                            paramList.Add($"{item.Key}={item.Value}");
                        }
                    }
                }

                // 4. 处理无参数的情况
                if (paramList.Count == 0)
                    return string.Empty;

                // 5. 拼接所有参数（限制总长度，避免日志过长）
                string allParams = string.Join("&", paramList);
                if (allParams.Length > 2000)
                {
                    return allParams.Substring(0, 2000) + "...[参数过长，已截断]";
                }

                return allParams;
            }
            catch (Exception ex)
            {
                return $"解析参数失败：{ex.Message.Substring(0, 100)}"; // 限制错误信息长度
            }
        }


    }
}
