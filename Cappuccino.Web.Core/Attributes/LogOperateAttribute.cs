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
        // 注入依赖服务
        public ISysLogOperateService LogOperateService { get; set; }

        /// <summary>
        /// 操作标题（如：新增用户）
        /// </summary>
        public string Title { get; set; } = "未命名操作";

        /// <summary>
        /// 业务类型（如：ADD/EDIT/DELETE/QUERY/EXPORT/IMPORT/AUTHORIZE/OTHER）
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
                var request = context.HttpContext.Request;  // 获取请求对象
                var response = context.HttpContext.Response; // 获取响应对象

                var user = UserManager.GetCurrentUserInfo(); // 当前登录用户
                if (user == null)
                {
                    user = context.Controller.TempData["UserInfo"] as SysUserEntity;
                }

                // 构建日志实体
                SysLogOperateEntity logOperateEntity = new SysLogOperateEntity
                {
                    // 基础配置信息
                    Title = Title,
                    Description = Description,
                    BusinessType = BusinessType,

                    // 请求信息
                    RequestMethod = request.HttpMethod,
                    RequestUrl = request.Url?.AbsolutePath,
                    Method = $"{context.Controller.GetType().Name}/{context.ActionDescriptor.ActionName}",
                    RequestParam = NetHelper.GetRequestParams(request), // 自定义方法：获取请求参数
                    RequestBody = request.HttpMethod.ToUpper() == "POST" ? NetHelper.GetRequestBody(request) : null,

                    // 请求结果
                    RequestResult = context.Exception == null ? response.Status : $"{response.Status} {context.Exception.Message}",

                    // 环境信息
                    IPAddress = NetHelper.GetIp,
                    IPAddressName = NetHelper.GetIpLocation(NetHelper.GetIp),
                    OperateName = user?.UserName,
                    SystemOs = NetHelper.GetSystemOs(request.UserAgent),
                    Browser = NetHelper.GetBrowser(request.UserAgent),
                    CreateUserId = user?.Id ?? 1
                };

                // 写入日志
                _ = LogOperateService?.WriteOperateLog(logOperateEntity);
            }
            catch (Exception ex)
            {
                // 日志记录失败不影响主业务，仅记录错误
                Log4netHelper.Error($"操作日志记录失败：{ex.Message}", ex);
            }
        }
    }
}