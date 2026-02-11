using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Util;
using Microsoft.AspNet.SignalR;

namespace Cappuccino.Web.Hubs
{
    public class ProcessProgressHub : Hub
    {
        /// <summary>
        /// 发送进度到前端（供Controller调用）
        /// </summary>
        public static void SendProgress(ProcessProgress progress)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ProcessProgressHub>();
            context.Clients.All.receiveProgress(progress);
        }
    }
}