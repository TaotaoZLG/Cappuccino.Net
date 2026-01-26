using System;
using System.Configuration;

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
            var cacheType = ConfigurationManager.AppSettings["CacheType"] ?? "HttpRuntime";

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

        public static void Set(string key, object value, TimeSpan cacheTime)
        {
            Cache.Set(key, value, cacheTime);
        }

        public static void Set(string key, object value)
        {
            Cache.Set(key, value);
        }
    }
}
