using System.Linq;
using Cappuccino.DataAccess;
using Cappuccino.Entity;
using Cappuccino.IDAL;

namespace Cappuccino.DAL.System
{
    public class SysNoticeDao : BaseDao<SysNoticeEntity>, ISysNoticeDao
    {
        public SysNoticeEntity GetByKey(long id)
        {
            return GetList(x => x.Id == id).FirstOrDefault();
        }
    }
}
