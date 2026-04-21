using System;
using System.Configuration;
using Cappuccino.Common.Enum;

namespace Cappuccino.Common.Caching
{
    /// <summary>
    /// 缓存管理类
    /// </summary>
    public class CacheManager
    {
        private static readonly Lazy<ICacheManager> _instance = new Lazy<ICacheManager>(CreateCacheManager);

        public static ICacheManager Cache => _instance.Value;

        public static ICacheManager CreateCacheManager()
        {
            // 从配置文件读取缓存类型
            var cacheType = ConfigurationManager.AppSettings["CacheType"] ?? "Memory";

            switch (cacheType.ToLower())
            {
                case "memory":
                    return new MemoryCacheManager();
                case "redis":
                    return new RedisCacheManager();
                case "httpruntime":
                default:
                    return new HttpRuntimeCacheManager();
            }
        }

        public static void Clear()
        {
            Cache.Clear();
        }

        public static bool Contains(string key)
        {
            return Cache.Contains(key);
        }

        public static T Get<T>(string key)
        {
            return Cache.Get<T>(key);
        }

        public static void Remove(string key)
        {
            Cache.Remove(key);
        }

        public static void Set(string key, object value, TimeSpan expiration)
        {
            Cache.Set(key, value, expiration);
        }

        public static void Set(string key, object value, TimeSpan expirationTime, CacheExpirationTypeEnum expirationType)
        {
            // 调用重载方法，指定「绝对过期」（解决记住密码闲置失效）
            Cache.Set(key, value, expirationTime, CacheExpirationTypeEnum.Absolute);
        }

        public static void Set(string key, object value)
        {
            Cache.Set(key, value);
        }
    }
}
