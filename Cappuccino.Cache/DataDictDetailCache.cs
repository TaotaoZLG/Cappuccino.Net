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
    public class DataDictDetailCache : BaseCache<SysDictDetailEntity>
    {
        private readonly ISysDictDetailDao _sysDictDetailDao;

        public override string CacheKey => KeyManager.DataDictDetailCache;

        public DataDictDetailCache(ISysDictDetailDao sysDictDetailDao)
        {
            _sysDictDetailDao = sysDictDetailDao;
        }

        public List<SysDictDetailEntity> GetList()
        {
            var cacheList = CacheManager.Get<List<SysDictDetailEntity>>(CacheKey);
            if (cacheList == null || !cacheList.Any())
            {
                var dictList = _sysDictDetailDao.GetList(x => true).ToList();
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
