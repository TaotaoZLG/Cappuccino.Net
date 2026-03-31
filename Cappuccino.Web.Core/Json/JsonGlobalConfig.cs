using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Cappuccino.Web.Core.Json
{
    /// <summary>
    /// 全局JSON序列化配置
    /// </summary>
    public static class JsonGlobalConfig
    {
        /// <summary>
        /// 全局统一的序列化设置
        /// </summary>
        public static JsonSerializerSettings Settings { get; set; }

        /// <summary>
        /// 初始化全局配置（在Web层启动时调用）
        /// </summary>
        public static void Init()
        {
            Settings = new JsonSerializerSettings
            {
                //忽略循环引用，如果设置为Error，则遇到循环引用的时候报错（建议设置为Error，这样更规范）
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                //日期格式化，默认的格式也不好看
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //json中属性开头字母小写的驼峰命名
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                //添加Long转换器（解决前端JS精度丢失）
                Converters = { new LongToStringConverter() }
            };

            // 关键：设置Newtonsoft.Json全局默认配置（影响JsonConvert.SerializeObject）
            JsonConvert.DefaultSettings = () => Settings;
        }
    }
}