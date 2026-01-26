using System.Threading.Tasks;
using Cappuccino.Entity;

namespace Cappuccino.IBLL.System
{
    public interface ISysAutoJobLogService : IBaseService<SysAutoJobLogEntity>
    {
        /// <summary>
        /// 记录任务执行日志
        /// </summary>
        /// <param name="entity">日志实体</param>
        /// <returns>影响行数</returns>
        int WriteJobLog(SysAutoJobLogEntity entity);

        /// <summary>
        /// 异步记录任务执行日志
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<int> WriteJobLogAsync(SysAutoJobLogEntity entity);
    }
}
