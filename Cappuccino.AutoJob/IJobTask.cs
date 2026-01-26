using System.Threading.Tasks;
using Cappuccino.Common.Util;

namespace Cappuccino.AutoJob
{
    /// <summary>
    /// 业务任务接口（供具体任务实现）
    /// </summary>
    public interface IJobTask
    {
        /// <summary>
        /// 执行任务
        /// </summary>
        Task<TData> Start();
    }
}
