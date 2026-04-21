using System;
using System.Web;
using System.Web.Caching;
using Cappuccino.Common.Enum;

namespace Cappuccino.Common.Caching
{
    public class HttpRuntimeCacheManager : ICacheManager
    {
        public void Clear()
        {
            var cache = HttpRuntime.Cache;
            var CacheEnum = cache.GetEnumerator();
            while (CacheEnum.MoveNext())
            {
                cache.Remove(CacheEnum.Key.ToString());
            }
        }

        public bool Contains(string key)
        {
            var data = HttpRuntime.Cache[key];
            if (data != null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public T Get<T>(string key)
        {
            return (T)HttpRuntime.Cache[key];
        }

        public object Get(string key)
        {
            return HttpRuntime.Cache[key];
        }

        public void Remove(string key)
        {
            HttpRuntime.Cache.Remove(key);
        }

        public void Set(string key, object value, TimeSpan cacheTime)
        {
            HttpRuntime.Cache.Insert(key, value, null, Cache.NoAbsoluteExpiration, cacheTime);
        }

        /// <summary>
        /// 将指定的键和对象添加到缓存（指定过期类型）
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expirationTime">过期时间</param>
        /// <param name="expirationType">过期类型（绝对/滑动）</param>
        public void Set(string key, object value, TimeSpan expirationTime, CacheExpirationTypeEnum expirationType)
        {
            if (string.IsNullOrEmpty(key) || value == null) return;

            // 根据过期类型设置缓存策略
            if (expirationType == CacheExpirationTypeEnum.Absolute)
            {
                // 绝对过期：从现在开始计算，到时间直接失效
                HttpRuntime.Cache.Insert(
                    key,
                    value,
                    null,
                    DateTime.Now.Add(expirationTime),
                    Cache.NoSlidingExpiration);
            }
            else
            {
                // 滑动过期：每次访问重置过期时间
                HttpRuntime.Cache.Insert(
                    key,
                    value,
                    null,
                    Cache.NoAbsoluteExpiration,
                    expirationTime);
            }
        }

        public void Set(string key, object value)
        {
            HttpRuntime.Cache.Insert(key, value);
        }
    }
}
