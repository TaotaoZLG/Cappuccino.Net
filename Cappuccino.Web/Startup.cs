using CLRStats;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Cappuccino.Web.Startup))]
namespace Cappuccino.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 https://go.microsoft.com/fwlink/?LinkID=316888
            app.UseCLRStatsDashboard();

            // 配置SignalR
            app.MapSignalR("/signalr", new HubConfiguration
            {
                EnableJSONP = true,
                EnableDetailedErrors = true
            });
        }
    }
}