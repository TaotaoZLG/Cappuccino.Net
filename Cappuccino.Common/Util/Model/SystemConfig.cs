using System;

namespace Cappuccino.Common.Util.Model
{
    public class SystemConfig
    {
        public SystemConfig() { }

        /// <summary>
        /// 允许一个用户在多个电脑同时登录
        /// </summary>
        public bool LoginMultiple { get; set; }

        public string LoginProvider { get; set; }

        /// <summary>
        /// Snow Flake Worker Id
        /// </summary>
        public int SnowFlakeWorkerId { get; set; }

        /// <summary>
        /// api地址
        /// </summary>
        public string ApiSite { get; set; }

        /// <summary>
        /// 允许跨域的站点
        /// </summary>
        public string AllowCorsSite { get; set; }

        /// <summary>
        /// 网站虚拟目录
        /// </summary>
        public string VirtualDirectory { get; set; }

        /// <summary>
        /// 数据库提供者
        /// </summary>
        public string DBProvider { get; set; }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string DBConnectionString { get; set; }

        /// <summary>
        ///  数据库超时间（秒）
        /// </summary>
        public int DBCommandTimeout { get; set; }

        /// <summary>
        /// 慢查询记录Sql(秒),保存到文件以便分析
        /// </summary>
        public int DBSlowSqlLogTime { get; set; }

        /// <summary>
        /// 数据库备份路径
        /// </summary>
        public string DBBackup { get; set; }

        /// <summary>
        /// 缓存提供者
        /// </summary>
        public string CacheProvider { get; set; }

        /// <summary>
        /// Redis连接字符串
        /// </summary>
        public string RedisConnectionString { get; set; }

        /// <summary>
        /// 并发数（默认保留一半的处理器核心数量）
        /// </summary>
        public int ConcurrencyNumber { get; set; } = (Environment.ProcessorCount / 2);

        /// <summary>
        /// HTTP请求超时时间(秒)
        /// </summary>
        public int HttpTimeout { get; set; }
    }
}