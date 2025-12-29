using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Cappuccino.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 路由
            config.MapHttpAttributeRoutes();

            // 默认路由
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // 配置JSON序列化（与现有Newtonsoft.Json兼容）
            config.Formatters.JsonFormatter.SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                DateFormatString = "yyyy-MM-dd HH:mm:ss"
            };

            // 启用跨域（如需前端访问）
            //config.EnableCors(); // 需先安装Microsoft.AspNet.WebApi.Cors
        }
    }
}
