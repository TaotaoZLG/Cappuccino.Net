using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Log;
using Cappuccino.Common.Util;
using Cappuccino.Model;
using Microsoft.AspNet.SignalR;

namespace Cappuccino.Web.Core
{
    public class ProcessProgressHub : Hub
    {
        /// <summary>
        /// 发送进度到前端
        /// </summary>
        public static void SendProgress(ProcessProgress progress)
        {
            try
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<ProcessProgressHub>();
                context.Clients.All.receiveProgress(progress);
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"SignalR推送失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 异步推送进度消息
        /// </summary>
        /// <param name="progress">进度模型</param>
        public static async Task SendProgressAsync(ProcessProgress progress)
        {
            try
            {
                // 获取Hub上下文，推送给所有客户端
                var context = GlobalHost.ConnectionManager.GetHubContext<ProcessProgressHub>();
                await context.Clients.All.receiveProgress(progress);
            }
            catch (Exception ex)
            {
                Log4netHelper.Error($"推送进度失败：{ex.Message}");
            }
        }
    }
}