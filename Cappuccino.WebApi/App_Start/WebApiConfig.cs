using System.Web.Http;
using Cappuccino.Web.Core.Filters;
using Cappuccino.Web.Core.Json;
using Cappuccino.WebApi.Filters;
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

            // 移除XML格式化器（只返回JSON，避免格式冲突）
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // 配置JSON序列化
            // 替换默认 JSON 格式化器为 Newtonsoft.Json，并应用统一配置
            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // 处理循环引用
                DateFormatString = "yyyy-MM-dd HH:mm:ss", // 日期格式化
                ContractResolver = new CamelCasePropertyNamesContractResolver() // 驼峰命名
            };

            // 注册异常过滤器
            config.Filters.Add(new ApiExceptionFilter());
            // 注册权限过滤器（如果需要全局生效）
            //config.Filters.Add(new ApiPermissionFilter());

            // 启用跨域（如需前端访问）
            //config.EnableCors(); // 需先安装Microsoft.AspNet.WebApi.Cors
        }
    }
}
