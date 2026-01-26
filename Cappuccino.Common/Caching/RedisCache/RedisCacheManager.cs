using System;
using System.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Cappuccino.Common.Caching
{
    /// <summary>
    /// 基于 Redis 的分布式缓存管理器
    /// </summary>
    public class RedisCacheManager : ICacheManager
    {
        private readonly IDatabase _database;
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        /// <summary>
        /// 构造函数，从配置文件读取连接字符串并初始化连接
        /// </summary>
        public RedisCacheManager()
        {
            // 从 Web.config 或 App.config 读取 Redis 连接字符串
            string connectionString = ConfigurationManager.AppSettings["RedisConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ConfigurationErrorsException("未在配置文件中找到 'RedisConnectionString' 配置项。");
            }

            // ConnectionMultiplexer 是 StackExchange.Redis 的核心，应被重用
            _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            _database = _connectionMultiplexer.GetDatabase();
        }

        /// <summary>
        /// 清除所有缓存（危险操作！）
        /// </summary>
        public void Clear()
        {
            // 警告：此操作会清空 Redis 中当前数据库的所有键！
            // 在生产环境中，最好为你的应用程序的所有键添加一个统一的前缀（如 "Cappuccino:"），
            // 然后只删除带有该前缀的键，以避免影响其他应用。
            var endpoints = _connectionMultiplexer.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _connectionMultiplexer.GetServer(endpoint);
                // 清空当前数据库
                server.FlushDatabase(_database.Database);
            }
        }

        /// <summary>
        /// 检查缓存项是否存在
        /// </summary>
        public bool Contains(string key)
        {
            return _database.KeyExists(key);
        }

        /// <summary>
        /// 获取缓存项
        /// </summary>
        public T Get<T>(string key)
        {
            RedisValue value = _database.StringGet(key);
            if (value.HasValue)
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            return default(T);
        }

        /// <summary>
        /// 获取缓存项
        /// </summary>
        public object Get(string key)
        {
            RedisValue value = _database.StringGet(key);
            if (value.HasValue)
            {
                // 注意：反序列化为 object 时，Json.NET 会返回 JObject/JArray 等类型
                return JsonConvert.DeserializeObject(value);
            }
            return null;
        }

        /// <summary>
        /// 移除指定的缓存项
        /// </summary>
        public void Remove(string key)
        {
            _database.KeyDelete(key);
        }

        /// <summary>
        /// 添加或更新缓存项，并设置过期时间
        /// </summary>
        public void Set(string key, object value, TimeSpan cacheTime)
        {
            if (value == null) return;

            string json = JsonConvert.SerializeObject(value);
            _database.StringSet(key, json, cacheTime);
        }

        /// <summary>
        /// 添加或更新缓存项，设置为永不过期
        /// </summary>
        public void Set(string key, object value)
        {
            if (value == null) return;

            string json = JsonConvert.SerializeObject(value);
            // 在 Redis 中，不设置过期时间即为永不过期
            _database.StringSet(key, json);
        }
    }
}