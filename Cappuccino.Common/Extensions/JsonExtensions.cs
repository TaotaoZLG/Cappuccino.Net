using System.Collections.Generic;
using System.Data;
using Cappuccino.Common.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Cappuccino.Common
{
    public static class JsonExtensions
    {
        // 全局默认JSON配置（添加Long转换器）
        private static readonly JsonSerializerSettings _defaultSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // 避免循环引用
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            Converters = { new LongToStringConverter() } // 核心：添加Long转换器
        };

        public static object ToJson(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject(Json);
        }

        // 重载1：使用带Long转换器的默认配置
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, _defaultSettings);
        }

        // 重载2：自定义时间格式 + 保留Long转换器
        public static string ToJson(this object obj, string datetimeformats)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateFormatString = datetimeformats,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            settings.Converters.Add(new LongToStringConverter()); // 加转换器
            return JsonConvert.SerializeObject(obj, settings);
        }

        // 反序列化也需适配（前端传字符串Id → 后端转long）
        public static T ToObject<T>(this string Json)
        {
            return Json == null ? default(T) : JsonConvert.DeserializeObject<T>(Json, _defaultSettings);
        }

        public static List<T> ToList<T>(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject<List<T>>(Json, _defaultSettings);
        }

        public static DataTable ToTable(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject<DataTable>(Json);
        }

        public static JObject ToJObject(this string Json)
        {
            return Json == null ? JObject.Parse("{}") : JObject.Parse(Json.Replace("&nbsp;", ""));
        }
    }
}