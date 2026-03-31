using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Cappuccino.Common.Caching
{
    /// <summary>
    /// 自定义Redis序列化器（处理循环引用）
    /// </summary>
    public class CustomRedisSerializer
    {
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // 核心：忽略循环引用
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// 序列化对象为Redis字节数组
        /// </summary>
        public static byte[] Serialize<T>(T obj)
        {
            if (obj == null) return null;
            string json = JsonConvert.SerializeObject(obj, _jsonSettings);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// 反序列化Redis字节数组为对象
        /// </summary>
        public static T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return default;
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
        }

        // 使用示例：存入Redis时用自定义序列化器
        public void SetRedisValue<T>(IDatabase db, string key, T value, TimeSpan? expiry = null)
        {
            byte[] data = CustomRedisSerializer.Serialize(value);
            db.StringSet(key, data, (Expiration)expiry);
        }

        public T GetRedisValue<T>(IDatabase db, string key)
        {
            byte[] data = db.StringGet(key);
            return CustomRedisSerializer.Deserialize<T>(data);
        }
    }
}