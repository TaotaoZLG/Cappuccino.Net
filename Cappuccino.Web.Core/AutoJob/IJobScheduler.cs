using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Web.Core.AutoJob
{
    /// <summary>
    /// 调度器接口
    /// </summary>
    public interface IJobScheduler
    {
        Task<string> Execute();

        bool StartJob(string jobKey, string cronExpression);
        bool StopJob(string jobKey);

        Task<bool> StartJobAsync(string jobKey, string cronExpression);
        Task<bool> StopJobAsync(string jobKey);
        Task<bool> TriggerJobImmediately(string jobKey);
    }
}
