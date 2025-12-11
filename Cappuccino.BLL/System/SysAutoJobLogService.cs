using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Net;
using Cappuccino.Entity;
using Cappuccino.Entity.System;
using Cappuccino.IBLL.System;
using Cappuccino.IDAL.System;
using Cappuccino.Web.Core;

namespace Cappuccino.BLL.System
{
    public class SysAutoJobLogService : BaseService<SysAutoJobLogEntity>, ISysAutoJobLogService
    {
        private readonly ISysAutoJobLogDao _logDao;

        public SysAutoJobLogService(ISysAutoJobLogDao logDao)
        {
            this._logDao = logDao;
            base.CurrentDao = logDao;
            AddDisposableObject(CurrentDao);
        }

        public int WriteJobLog(SysAutoJobLogEntity entity)
        {
            entity.CreateTime = DateTime.Now;
            entity.CreateUserId = UserManager.GetCurrentUserInfo()?.Id ?? 0;
            return Insert(entity);
        }

        /// <summary>
        /// 异步记录任务执行日志
        /// </summary>
        /// <param name="entity">日志实体</param>
        /// <returns>影响的行数</returns>
        public async Task<int> WriteJobLogAsync(SysAutoJobLogEntity entity)
        {
            entity.CreateTime = DateTime.Now;
            return await InsertAsync(entity);
        }
    }
}
