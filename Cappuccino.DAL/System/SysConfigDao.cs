using System.Linq;
using Cappuccino.DataAccess;
using Cappuccino.Entity;
using Cappuccino.IDAL;

namespace Cappuccino.DAL.System
{
    public class SysConfigDao : BaseDao<SysConfigEntity>, ISysConfigDao
    {
        public SysConfigEntity GetByConfig(string key)
        {
            return GetList(x => x.ConfigKeys == key).FirstOrDefault();
        }
    }
}
