using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common;
using Cappuccino.Common.Caching;
using Cappuccino.Entity;
using Cappuccino.IDAL;
using Cappuccino.Model;

namespace Cappuccino.Cache
{
    public class ActionMenuCache : BaseCache<SysActionEntity>
    {
        private readonly ISysActionDao _sysActionDao;

        public override string CacheKey => KeyManager.ActionMenuCache;

        public ActionMenuCache(ISysActionDao sysActionDao)
        {
            _sysActionDao = sysActionDao;
        }

        public List<SysActionEntity> GetList()
        {
            var cacheList = CacheManager.Get<List<SysActionEntity>>(CacheKey);
            if (cacheList == null || !cacheList.Any())
            {
                var dictList = _sysActionDao.GetList(x => true).ToList();
                CacheManager.Set(CacheKey, dictList);
                return dictList;
            }
            else
            {
                return cacheList;
            }
        }
    }
}
