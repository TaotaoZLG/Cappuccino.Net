using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Mvc;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Helper;
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
        /// 业务类型（OperateType枚举值）
        /// </summary>
        public int BusinessType { get; set; } = 0;

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

                var user = UserManager.GetCurrentUserInfo() ?? context.Controller.TempData["UserInfo"] as SysUserEntity; // 当前登录用户
                string requestParams = NetHelper.GetRequestParams(request);
                string loginName = TextHelper.ExtractParamValue(requestParams, "LoginName");

                // 构建日志实体
                SysLogOperateEntity logOperateEntity = new SysLogOperateEntity();

                // 处理异常场景（优先返回异常信息）
                var stringBuilder = new StringBuilder();
                if (context.Exception != null)
                {
                    Exception innerEx = context.Exception.InnerException ?? context.Exception;
                    while (innerEx.InnerException != null)
                    {
                        innerEx = innerEx.InnerException;
                    }
                    stringBuilder.Append(innerEx.Message);

                    logOperateEntity.LogStatus = 0;
                }
                else
                {
                    var result = context.Result as JsonResult;
                    stringBuilder.Append(result?.Data?.ToString());

                    logOperateEntity.LogStatus = 1;
                }

                // 基础配置信息
                logOperateEntity.Title = attribute.Title;
                logOperateEntity.Description = attribute.Description;
                logOperateEntity.BusinessType = attribute.BusinessType;

                // 请求信息
                logOperateEntity.RequestMethod = request.HttpMethod;
                logOperateEntity.RequestUrl = request.Url?.AbsolutePath;
                logOperateEntity.Method = $"{context.Controller.GetType().Name}/{context.ActionDescriptor.ActionName}";
                logOperateEntity.RequestParam = requestParams;
                logOperateEntity.RequestBody = request.HttpMethod.ToUpper() == "POST" ? NetHelper.GetRequestBody(request) : null;

                // 请求结果
                logOperateEntity.RequestResult = stringBuilder.ToString();

                // 环境信息
                logOperateEntity.IPAddress = NetHelper.GetIp;
                logOperateEntity.IPAddressName = NetHelper.GetIpLocation(NetHelper.GetIp);
                logOperateEntity.OperateName = user?.UserName ?? loginName;
                logOperateEntity.SystemOs = NetHelper.GetSystemOs(request.UserAgent);
                logOperateEntity.Browser = NetHelper.GetBrowser(request.UserAgent);
                logOperateEntity.CreateUserId = user?.Id ?? 1;

                Action action = async () =>
                {
                    // 写入日志
                    await LogOperateService?.WriteOperateLogAsync(logOperateEntity);
                };
                AsyncTaskHelper.StartTask(action);
            }
            catch (Exception ex)
            {
                // 日志记录失败不影响主业务，仅记录错误
                Log4netHelper.Error($"操作日志记录失败：{ex.Message}", ex);
            }
        }
    }
}