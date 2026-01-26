using System.Web.Mvc;
using Cappuccino.Web.Core.Filters;

namespace Cappuccino.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //自定义异常
            filters.Add(new MyExceptionAttribute());
            //权限验证过滤器
            filters.Add(new CheckPermissionFilterAttribute());
            //注册IP黑名单过滤器
            //filters.Add(new IpBlackListFilterAttribute());
            //Json.Net(Newtonsoft.Json)和 ASP.net MVC 的结合
            filters.Add(new JsonNetActionFilter());
        }
    }
}
