using System.Web;
using System.Web.Http;

namespace Cappuccino.WebApi
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // 注册Web API路由
            GlobalConfiguration.Configure(WebApiConfig.Register);
            // 注册Autofac
            AutofacConfig.Register();
            // 注册Swagger
            SwaggerConfig.Register();
        }
    }
}
