using System;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Cappuccino.AutoJob;
using Cappuccino.Common.Log;
using Cappuccino.Web.Core.Filters;

namespace Cappuccino.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            //注册区域路由规则
            AreaRegistration.RegisterAllAreas();
            //注册全局过滤器
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            //注册网站路由
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //优化css js
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //注册autofac 
            AutofacConfig.Register();
            //开启一个线程，扫描异常信息队列。
            ThreadPool.QueueUserWorkItem((a) =>
            {
                while (true)
                {
                    //判断一下队列中是否有数据
                    if (MyExceptionAttribute.ExecptionQueue.Count() > 0)
                    {
                        Exception ex = MyExceptionAttribute.ExecptionQueue.Dequeue();
                        if (ex != null)
                        {
                            Log4netHelper.Error(ex.ToString());
                        }
                        else
                        {
                            //如果队列中没有数据，休息
                            Thread.Sleep(3000);
                        }
                    }
                    else
                    {
                        //如果队列中没有数据，休息
                        Thread.Sleep(3000);
                    }
                }
            });

            // 启动Quartz调度器
            //new JobScheduler().Start().GetAwaiter().GetResult();
            // 从Autofac容器获取单例调度器并启动（仅启动一次）
            var scheduler = DependencyResolver.Current.GetService<IJobScheduler>();
            scheduler.Start().GetAwaiter().GetResult();
        }
    }
}
