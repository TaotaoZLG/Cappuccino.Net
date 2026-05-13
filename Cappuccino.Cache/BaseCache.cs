using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common;
using Cappuccino.Common.Caching;

namespace Cappuccino.Cache
{
    public abstract class BaseCache<T>
    {
        public abstract string CacheKey { get; }

        public virtual void ClearCache()
        {
            CacheManager.Remove(CacheKey);
        }
    }
}
