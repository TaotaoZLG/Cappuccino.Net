using System;
using System.Linq;
using System.Web.Mvc;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Log;
using Cappuccino.Common.Net;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.Web.Core;

namespace Cappuccino.Web.Attributes
{
    /// <summary>
    /// MVC操作日志AOP特性（合并标记与拦截逻辑）
    /// 用法：在需要记录日志的Action上添加 [LogOperate] 并配置参数
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class LogOperateAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 操作标题（如：新增用户）
        /// </summary>
        public string Title { get; set; } = "未命名操作";

        /// <summary>
        /// 业务类型（如：ADD/EDIT/DELETE/QUERY）
        /// </summary>
        public string BusinessType { get; set; } = "OTHER";

        /// <summary>
        /// 操作描述（详细说明）
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 是否忽略空参数（不记录无参数的请求）
        /// </summary>
        public bool IgnoreEmptyParam { get; set; } = false;


        #region 依赖服务（通过DI注入）
        /// <summary>
        /// 日志业务服务（由MVC容器注入）
        /// </summary>
        public ISysLogOperateService LogOperateService { get; set; }
        #endregion

        /// <summary>
        /// Action执行完成后记录日志
        /// </summary>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            // 获取方法上的日志特性
            LogOperateAttribute attribute = context.ActionDescriptor.GetCustomAttributes(typeof(LogOperateAttribute), true).FirstOrDefault() as LogOperateAttribute;
            if (attribute == null) return;

            try
            {
                var request = context.HttpContext.Request;
                var user = UserManager.GetCurrentUserInfo(); // 当前登录用户

                // 构建日志实体
                var log = new SysLogOperateEntity
                {
                    // 基础配置信息
                    Title = Title,
                    Description = Description,
                    BusinessType = BusinessType,

                    // 请求信息
                    RequestMethod = request.HttpMethod,
                    OperateUrl = request.Url?.AbsolutePath ?? string.Empty,
                    Method = $"{context.Controller.GetType().Name}.{context.ActionDescriptor.ActionName}",
                    RequestParam = NetHelper.GetRequestParams(request), // 自定义方法：获取请求参数
                    RequestBody = request.HttpMethod.ToUpper() == "POST" ? NetHelper.GetRequestBody(request) : null,

                    // 执行结果
                    Success = context.Exception == null,
                    ErrorMsg = context.Exception?.Message.TruncateString(500) ?? string.Empty,

                    // 环境信息
                    IPAddress = NetHelper.GetIp,
                    IPAddressName = NetHelper.GetIpLocation(NetHelper.GetIp),
                    OperateName = user?.UserName,
                    SystemOs = NetHelper.GetSystemOs(request.UserAgent),
                    Browser = NetHelper.GetBrowser(request.UserAgent),
                    CreateTime = DateTime.Now
                };

                // 写入日志（异步执行，不阻塞主流程）
                _ = LogOperateService?.WriteOperateLog(log);
            }
            catch (Exception ex)
            {
                // 日志记录失败不影响主业务，仅记录错误
                Log4netHelper.Error($"操作日志记录失败：{ex.Message}", ex);
            }
        }
    }
}