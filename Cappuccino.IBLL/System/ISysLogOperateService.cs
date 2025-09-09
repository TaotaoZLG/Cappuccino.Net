using System.Threading.Tasks;
using Cappuccino.Entity;

namespace Cappuccino.IBLL
{
    public interface ISysLogOperateService : IBaseService<SysLogOperateEntity>
    {
        /// <summary>
        /// 写入操作日志
        /// </summary>
        /// <param name="logOperate">操作日志实体</param>
        /// <returns>影响行数</returns>
        int WriteOperateLog(SysLogOperateEntity logOperate);

        Task<int> WriteOperateLogAsync(SysLogOperateEntity logOperate);
    }
}
