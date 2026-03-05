using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Cappuccino.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cappuccino.Common.Helper
{
    #region JsonHelper
    public static class JsonHelper
    {
        /// <summary>
        /// 将对象序列化为JSON格式
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>json字符串</returns>
        public static string SerializeObject(object o)
        {
            string json = JsonConvert.SerializeObject(o);
            return json;
        }

        /// <summary>
		///  将DataTable转换为Json
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string DataTableToJson(DataTable dt)
        {
            string JsonString = string.Empty;
            if (dt.Rows.Count > 0)
            {
                return JsonString = JsonConvert.SerializeObject(dt);
            }
            return null;
        }

        /// <summary>
        /// 解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串(eg.{"ID":"112","Name":"石子儿"})</param>
        /// <returns>对象实体</returns>
        public static T DeserializeJsonToObject<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
            T t = o as T;
            return t;
        }

        public static T ToObject<T>(this string Json)
        {
            Json = Json.Replace("&nbsp;", "");
            return Json == null ? default(T) : JsonConvert.DeserializeObject<T>(Json);
        }

        public static JObject ToJObject(this string Json)
        {
            return Json == null ? JObject.Parse("{}") : JObject.Parse(Json.Replace("&nbsp;", ""));
        }

        /// <summary>
        /// 解析JSON数组生成对象实体集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json数组字符串(eg.[{"ID":"112","Name":"石子儿"}])</param>
        /// <returns>对象实体集合</returns>
        public static List<T> DeserializeJsonToList<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            List<T> list = o as List<T>;
            return list;
        }

        /// <summary>
        /// 反序列化JSON到给定的匿名对象.
        /// </summary>
        /// <typeparam name="T">匿名对象类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <param name="anonymousTypeObject">匿名对象</param>
        /// <returns>匿名对象</returns>
        public static T DeserializeAnonymousType<T>(string json, T anonymousTypeObject)
        {
            T t = JsonConvert.DeserializeAnonymousType(json, anonymousTypeObject);
            return t;
        }

        /// <summary>
        /// Entity转JSON
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string EntityToJSON(object obj)
        {
            JsonSerializerSettings jsetting = new JsonSerializerSettings();
            jsetting.NullValueHandling = NullValueHandling.Ignore;
            string result = JsonConvert.SerializeObject(obj, Formatting.None, jsetting);
            return result;
        }

        /// <summary>
        /// JSON对象转JSON字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToString(object obj)
        {
            JsonSerializerSettings jsetting = new JsonSerializerSettings();
            jsetting.NullValueHandling = NullValueHandling.Ignore;
            string result = JsonConvert.SerializeObject(obj, Formatting.None, jsetting);
            return result;
        }
    }
    #endregion

    #region JsonConverter
    /// <summary>
    /// Json数据返回到前端js的时候，把数值很大的long类型转成字符串
    /// </summary>
    public class StringJsonConverter : JsonConverter
    {
        public StringJsonConverter() { }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value.ParseToLong();
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            string sValue = value.ToString();
            writer.WriteValue(sValue);
        }
    }

    /// <summary>
    /// DateTime类型序列化的时候，转成指定的格式
    /// </summary>
    public class DateTimeJsonConverter : JsonConverter
    {
        public DateTimeJsonConverter() { }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value.ParseToString().ParseToDateTime();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            DateTime? dt = value as DateTime?;
            if (dt == null)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteValue(dt.Value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
    #endregion
}
