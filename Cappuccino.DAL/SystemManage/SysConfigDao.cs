using System.Linq;
using Cappuccino.DataAccess;
using Cappuccino.Entity;
using Cappuccino.IDAL;

namespace Cappuccino.DAL.System
{
    public class SysConfigDao : BaseDao<SysConfigEntity>, ISysConfigDao
    {
        public SysConfigEntity GetConfigByKey(string configKey)
        {
            return GetList(x => x.ConfigKeys == configKey).FirstOrDefault();
        }
    }
}
