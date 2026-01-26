using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Cappuccino.Web.Core.Filters
{
    public class ApiPermissionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // 权限验证逻辑（示例：未通过验证）
            bool hasPermission = false;
            if (!hasPermission)
            {
                // 返回统一格式的无权限响应
                actionContext.Response = actionContext.Request.CreateResponse(
                    System.Net.HttpStatusCode.Forbidden,
                    "您没有执行此操作的权限"
                );
            }
        }
    }
}