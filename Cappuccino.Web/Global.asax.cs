using Cappuccino.Common.Log;
using Cappuccino.Web.Core.Filters;
using System;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Cappuccino.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            //ע������·�ɹ���
            AreaRegistration.RegisterAllAreas();
            //ע��ȫ�ֹ�����
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            //ע����վ·��
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //�Ż�css js
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //ע��autofac 
            AutofacConfig.Register();
            //����һ���̣߳�ɨ���쳣��Ϣ���С�
            ThreadPool.QueueUserWorkItem((a) =>
            {
                while (true)
                {
                    //�ж�һ�¶������Ƿ�������
                    if (MyExceptionAttribute.ExecptionQueue.Count() > 0)
                    {
                        Exception ex = MyExceptionAttribute.ExecptionQueue.Dequeue();
                        if (ex != null)
                        {
                            Log4netHelper.Error(ex.ToString());
                        }
                        else
                        {
                            //���������û�����ݣ���Ϣ
                            Thread.Sleep(3000);
                        }
                    }
                    else
                    {
                        //���������û�����ݣ���Ϣ
                        Thread.Sleep(3000);
                    }
                }
            });
        }
    }
}
