using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            _logDao = logDao;
            base.CurrentDao = logDao;
            AddDisposableObject(CurrentDao);
        }

        public int WriteJobLog(SysAutoJobLogEntity log)
        {
            log.CreateTime = DateTime.Now;
            log.CreateUserId = UserManager.GetCurrentUserInfo()?.Id ?? 0;
            return Add(log);
        }
    }
}
