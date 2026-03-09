using System.Threading.Tasks;
using Cappuccino.Entity;

namespace Cappuccino.IBLL.System
{
    public interface ISysAutoJobService : IBaseService<SysAutoJobEntity>
    {
        /// <summary>
        /// 启动任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>是否启动成功</returns>
        Task<bool> StartJob(long id);

        /// <summary>
        /// 停止任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>是否停止成功</returns>
        Task<bool> StopJob(long id);

        /// <summary>
        /// 立即执行任务
        /// </summary>
        /// <param name="id">任务ID</param>
        /// <returns>是否执行成功</returns>
        Task<bool> ExecuteJob(long id);
    }
}
