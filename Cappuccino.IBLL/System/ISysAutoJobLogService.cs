using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Entity.System;

namespace Cappuccino.IBLL.System
{
    public interface ISysAutoJobLogService : IBaseService<SysAutoJobLogEntity>
    {
        /// <summary>
        /// 记录任务执行日志
        /// </summary>
        /// <param name="log">日志实体</param>
        /// <returns>影响行数</returns>
        int WriteJobLog(SysAutoJobLogEntity log);
    }
}
