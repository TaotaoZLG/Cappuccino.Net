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

            // 1. 初始化雪花算法（关键：不同服务器/实例需配置不同的WorkerId，范围0-31）
            // 示例：测试环境WorkerId=1，生产环境可通过配置文件/环境变量读取
            var workerId = long.TryParse(ConfigurationManager.AppSettings["Snowflake.WorkerId"], out var w) ? w : 1;
            var datacenterId = long.TryParse(ConfigurationManager.AppSettings["Snowflake.DatacenterId"], out var d) ? d : 1;

            // 必须在任何地方调用 Instance 之前执行
            IdGeneratorHelper.Instance.SetConfig(workerId, datacenterId);

            // 从容器中获取调度器并启动
            var _jobCenter = DependencyResolver.Current.GetService<JobCenter>();
            _jobCenter.Start();
        }
    }
}
