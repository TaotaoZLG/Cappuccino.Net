using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Filters;

namespace Cappuccino.WebApi.Filters
{
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            // 捕获异常并返回统一格式的错误响应
            var ex = context.Exception;
            var innerEx = ex.InnerException ?? ex;
            while (innerEx.InnerException != null)
                innerEx = innerEx.InnerException;

            context.Response = context.Request.CreateResponse(
                System.Net.HttpStatusCode.InternalServerError,
                innerEx.Message
            );
        }
    }
}
