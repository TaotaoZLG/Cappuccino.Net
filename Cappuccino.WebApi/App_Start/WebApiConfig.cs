using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings
            {
                //忽略循环引用，如果设置为Error，则遇到循环引用的时候报错（建议设置为Error，这样更规范）
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                //日期格式化，默认的格式也不好看
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //json中属性开头字母小写的驼峰命名
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            // 移除XML格式化器（只返回JSON，避免格式冲突）
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // 启用跨域（如需前端访问）
            //config.EnableCors(); // 需先安装Microsoft.AspNet.WebApi.Cors
        }
    }
}
