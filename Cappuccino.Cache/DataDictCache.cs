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
    public class DataDictCache : BaseCache<SysDictEntity>
    {
        private readonly ISysDictDao _sysDictDao;

        public override string CacheKey => KeyManager.DataDictCache;

        public DataDictCache(ISysDictDao sysDictDao)
        {
            _sysDictDao = sysDictDao;
        }

        public List<SysDictEntity> GetList()
        {
            var cacheList = CacheManager.Get<List<SysDictEntity>>(CacheKey);
            if (cacheList == null || !cacheList.Any())
            {
                var dictList = _sysDictDao.GetList(x => true).ToList();
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
