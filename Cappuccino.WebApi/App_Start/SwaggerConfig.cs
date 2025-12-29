using System.Web.Http;
using WebActivatorEx;
using Cappuccino.WebApi;
using Swashbuckle.Application;
using System;

namespace Cappuccino.WebApi
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                {
                    // API文档标题和版本
                    c.SingleApiVersion("v1", "Cappuccino.WebApi");

                    // 启用XML注释（可选，需在项目属性中配置）
                    c.IncludeXmlComments(GetXmlCommentsPath());
                })
                .EnableSwaggerUi(c =>
                {
                    // 自定义Swagger UI样式（可选）
                    c.DocumentTitle("Cappuccino API文档");
                });
        }

        // 获取XML注释文件路径（需在项目属性中启用）
        private static string GetXmlCommentsPath()
        {
            return $@"{AppDomain.CurrentDomain.BaseDirectory}\Cappuccino.WebApi.XML";
        }
    }
}
