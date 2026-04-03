using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Cappuccino.Common.Log;
using Cappuccino.Common.Util.Model;
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
            // 1. 初始化Autofac容器
            Container = container;

            // 2. 加载系统配置（从 system.config 自动解析）
            //SystemConfig = SystemConfig.LoadFromConfig();
            //Log4netHelper.Info($"系统配置加载完成：缓存类型={SystemConfig.CacheType}，上传文件大小限制={SystemConfig.UploadMaxFileSize / 1024 / 1024}MB");
        }
    }
}
