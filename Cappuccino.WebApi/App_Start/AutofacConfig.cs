using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Cappuccino.BLL.System;
using Cappuccino.IBLL;

namespace Cappuccino.WebApi
{
    public class AutofacConfig
    {
        public static void Register()
        {
            var builder = new ContainerBuilder();
            // 注册Web API控制器
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // 注册业务层和数据层（复用现有逻辑）
            builder.RegisterTypes(Assembly.Load("Cappuccino.BLL").GetTypes())
                   .AsImplementedInterfaces();
            builder.RegisterTypes(Assembly.Load("Cappuccino.DAL").GetTypes())
                   .AsImplementedInterfaces();

            // 注册其他服务（如日志、权限等，参考Cappuccino.Web的AutofacConfig）
            builder.RegisterType<SysLogOperateService>().As<ISysLogOperateService>()
                   .InstancePerRequest();

            var container = builder.Build();
            // 设置Web API依赖解析器
            GlobalConfiguration.Configuration.DependencyResolver =
                new AutofacWebApiDependencyResolver(container);
        }
    }
}