using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Cappuccino.Common.Log;
using Cappuccino.Common.Util;
using Newtonsoft.Json;

namespace Cappuccino.Common
{
    public class GlobalContext
    {
        /// <summary>
        /// 全局Autofac容器实例（静态持有，不序列化）
        /// </summary>
        //[JsonIgnore]
        public static IContainer Container { get; private set; }

        public static SystemConfig SystemConfig { get; set; }

        /// <summary>
        /// 初始化全局上下文（容器 + 配置）
        /// </summary>
        /// <param name="container">Autofac构建好的容器</param>
        public static void Initialize(IContainer container)
        {
            // 初始化Autofac容器
            Container = container;

            // 初始化系统配置（从 system.config 自动解析）
            SystemConfig = new SystemConfig();

            // 读取Web.config中引用的system.config（推荐，自动适配部署环境）
            var appSettings = ConfigurationManager.AppSettings;
            foreach (string key in appSettings.AllKeys)
            {
                var value = appSettings[key];
                SystemConfig.SetValue(key, value);
            }
            //Log4netHelper.Info($"系统配置加载完成：缓存类型={SystemConfig.CacheType}，上传文件大小限制={SystemConfig.UploadMaxFileSize / 1024 / 1024}MB");
        }
    }
}
