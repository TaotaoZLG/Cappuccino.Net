using System.Reflection;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Cappuccino.AutoJob;
using Cappuccino.BLL;
using Cappuccino.Common;
using Cappuccino.Common.Caching;
using Cappuccino.Common.Log;
using Cappuccino.IBLL;

namespace Cappuccino.Web
{
    /// <summary>
    /// autofac配置
    /// </summary>
    public class AutofacConfig
    {
        /// <summary>
        /// 负责调用autofac框架实现业务逻辑层和数据仓储层程序集中的类型对象的创建
        /// 负责创建MVC控制器类的对象(调用控制器中的有参构造函数),接管DefaultControllerFactory的工作
        /// </summary>
        public static void Register()
        {
            //实例化一个autofac的创建容器
            var builder = new ContainerBuilder();

            //告诉Autofac框架，将来要创建的控制器类存放在哪个程序集 (Cappuccino.Web)
            Assembly controllerAss = Assembly.Load("Cappuccino.Web");
            builder.RegisterControllers(controllerAss);

            //告诉autofac框架注册数据仓储层所在程序集中的所有类的对象实例
            Assembly respAss = Assembly.Load("Cappuccino.DAL");
            //创建DAL中的所有类的instance以此类的实现接口存储
            builder.RegisterTypes(respAss.GetTypes()).AsImplementedInterfaces();

            //告诉autofac框架注册业务逻辑层所在程序集中的所有类的对象实例
            Assembly serAss = Assembly.Load("Cappuccino.BLL");
            //创建BLL中的所有类的instance以此类的实现接口存储
            builder.RegisterTypes(serAss.GetTypes()).AsImplementedInterfaces();

            Assembly cacheAss = Assembly.Load("Cappuccino.Cache");
            //创建Cache中的所有类
            builder.RegisterTypes(cacheAss.GetTypes()).AsSelf();

            // 注册MVC控制器并启用过滤器属性注入
            builder.RegisterControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();

            // 注册过滤器提供者
            builder.RegisterFilterProvider();

            // 注册Quartz为单例
            builder.RegisterType<JobExecutor>().AsSelf().SingleInstance();
            builder.RegisterType<JobScheduler>().As<IJobScheduler>().SingleInstance();
            builder.RegisterType<JobCenter>().AsSelf().SingleInstance();

            //创建一个Autofac的容器
            var container = builder.Build();
            //将container对象缓存起来，并永久有效
            //CacheManager.Set(KeyManager.AutofacContainer, container);

            //用静态类持有容器，而非存入缓存
            GlobalContext.Initialize(container);
            //设置依赖解析器 将MVC的控制器对象实例 交由autofac来创建
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}