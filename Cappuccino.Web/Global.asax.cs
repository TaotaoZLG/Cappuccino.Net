using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Cappuccino.AutoJob;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Log;
using Cappuccino.Web.Core.Filters;
using Cappuccino.Web.Core.Json;

namespace Cappuccino.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            //优先初始化全局JSON序列化配置（必须在过滤器注册前）
            JsonGlobalConfig.Init();

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

            // 从容器中获取调度器并启动
            var _jobCenter = DependencyResolver.Current.GetService<JobCenter>();
            _jobCenter.Start();
        }
    }
}
