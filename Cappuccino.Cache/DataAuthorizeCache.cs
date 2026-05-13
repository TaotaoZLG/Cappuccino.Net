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
    public class DataAuthorizeCache : BaseCache<SysDataAuthorizeEntity>
    {
        private readonly ISysDataAuthorizeDao _sysDataAuthorizeDao;

        public override string CacheKey => KeyManager.DataPermission;

        public DataAuthorizeCache(ISysDataAuthorizeDao sysDataAuthorizeDao)
        { 
            _sysDataAuthorizeDao = sysDataAuthorizeDao;
        }

        public HashSet<SysDataAuthorizeEntity> GetList()
        {
            var cacheList = CacheManager.Get<HashSet<SysDataAuthorizeEntity>>(CacheKey);
            if (cacheList == null || !cacheList.Any())
            {
                var dataAuthorizeList = _sysDataAuthorizeDao.GetAuthorizeList();
                CacheManager.Set(CacheKey, dataAuthorizeList);
                return dataAuthorizeList;
            }
            else
            {
                return cacheList;
            }
        }

    }
}
