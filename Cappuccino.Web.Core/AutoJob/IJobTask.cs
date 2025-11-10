using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace Cappuccino.Web.Core.AutoJob
{
    /// <summary>
    /// 业务任务接口（供具体任务实现）
    /// </summary>
    public interface IJobTask
    {
        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="jobId">任务ID</param>
        /// <param name="parameters">任务参数</param>
        Task Execute(int jobId, object parameters = null);
    }
}
