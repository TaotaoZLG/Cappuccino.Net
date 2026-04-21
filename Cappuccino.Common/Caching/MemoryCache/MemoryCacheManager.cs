using System;
using System.Runtime.Caching;
using Cappuccino.Common.Enum;

namespace Cappuccino.Common.Caching
{
    public class MemoryCacheManager : ICacheManager
    {
        // 使用 .NET 内置的默认 MemoryCache 实例
        private static readonly ObjectCache _cache = MemoryCache.Default;

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void Clear()
        {
            // 遍历并移除所有缓存项
            // 注意：在高并发下这样做可能有性能问题，但对于管理后台的操作是可接受的
            foreach (var item in _cache)
            {
                _cache.Remove(item.Key);
            }
        }

        /// <summary>
        /// 检查缓存项是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>如果存在则返回 true，否则返回 false</returns>
        public bool Contains(string key)
        {
            return _cache.Contains(key);
        }

        /// <summary>
        /// 获取缓存项
        /// </summary>
        /// <typeparam name="T">缓存值的类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存的值，如果不存在则返回类型的默认值</returns>
        public T Get<T>(string key)
        {
            return (T)_cache.Get(key);
        }

        /// <summary>
        /// 获取缓存项
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存的对象，如果不存在则返回 null</returns>
        public object Get(string key)
        {
            return _cache.Get(key);
        }

        /// <summary>
        /// 移除指定的缓存项
        /// </summary>
        /// <param name="key">缓存键</param>
        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        /// <summary>
        /// 添加或更新缓存项，并设置滑动过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expiration">滑动过期时长</param>
        public void Set(string key, object value, TimeSpan expiration)
        {
            var policy = new CacheItemPolicy();
            policy.SlidingExpiration = expiration;
            // 高优先级：防止内存压力下被主动清理
            policy.Priority = CacheItemPriority.NotRemovable;

            _cache.Set(new CacheItem(key, value), policy);
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

            var cachePolicy = new CacheItemPolicy
            {
                // 高优先级：防止内存压力下被主动清理
                Priority = CacheItemPriority.NotRemovable
            };

            // 设置过期策略
            if (expirationType == CacheExpirationTypeEnum.Absolute)
            {
                cachePolicy.AbsoluteExpiration = DateTime.Now.Add(expirationTime);
            }
            else
            {
                cachePolicy.SlidingExpiration = expirationTime;
            }

            // 写入缓存
            _cache.Set(key, value, cachePolicy);
        }

        /// <summary>
        /// 添加或更新缓存项，设置为永不过期
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        public void Set(string key, object value)
        {
            // 设置一个非常久远的绝对过期时间来模拟永不过期
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.MaxValue };
            _cache.Set(new CacheItem(key, value), policy);
        }
    }
}
