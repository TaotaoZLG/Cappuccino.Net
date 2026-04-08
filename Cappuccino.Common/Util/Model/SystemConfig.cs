using System;
using System.Security.AccessControl;
using Cappuccino.Common.Extensions;

namespace Cappuccino.Common.Util
{
    /// <summary>
    /// 系统配置实体（映射system.config）
    /// </summary>
    public class SystemConfig
    {
        /// <summary>
        /// 允许一个用户在多个电脑同时登录
        /// </summary>
        public bool LoginMultiple { get; set; }

        /// <summary>
        /// 登录提供者（默认使用Cookie，也可以配置为其他方式）
        /// </summary>
        public string LoginProvider { get; set; }

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
        /// 并发数（默认保留一半的处理器核心数量）
        /// </summary>
        public int ConcurrencyNumber { get; set; } = (Environment.ProcessorCount / 2);

        /// <summary>
        /// HTTP请求超时时间(秒)
        /// </summary>
        public int HttpTimeout { get; set; }

        #region 数据库配置
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
        public string DbBackupPath { get; set; }
        #endregion       

        #region 1. MVC基础配置
        /// <summary>
        /// WebPages版本
        /// </summary>
        public string WebpagesVersion { get; set; }

        /// <summary>
        /// 是否启用WebPages
        /// </summary>
        public bool WebpagesEnabled { get; set; }

        /// <summary>
        /// 是否启用客户端验证
        /// </summary>
        public bool ClientValidationEnabled { get; set; }

        /// <summary>
        /// 是否启用非侵入式JS验证
        /// </summary>
        public bool UnobtrusiveJavaScriptEnabled { get; set; }
        #endregion

        #region 2. 缓存配置
        /// <summary>
        /// 缓存提供者（Memory/Redis/HttpRuntime）
        /// </summary>
        public string CacheProvider { get; set; }

        /// <summary>
        /// Redis连接字符串
        /// </summary>
        public string RedisConnectionString { get; set; }
        #endregion

        #region 3. 文件上传配置
        /// <summary>
        /// 图片上传大小限制（KB）
        /// </summary>
        public int UploadFileByImgSize { get; set; }

        /// <summary>
        /// 图片上传允许类型
        /// </summary>
        public string UploadFileByImgType { get; set; }

        /// <summary>
        /// 上传文件最大大小（字节）
        /// </summary>
        public long UploadMaxFileSize { get; set; }

        /// <summary>
        /// 压缩包支持格式
        /// </summary>
        public string CompressedFileFormats { get; set; }

        /// <summary>
        /// 图片支持格式
        /// </summary>
        public string ImageFileFormats { get; set; }
        #endregion

        #region 4. 临时文件配置
        /// <summary>
        /// 临时文件根目录
        /// </summary>
        public string TempRootPath { get; set; }

        /// <summary>
        /// 归档文件根目录
        /// </summary>
        public string ArchiveRootPath { get; set; }

        /// <summary>
        /// 导出文件根目录
        /// </summary>
        public string ExportRootPath { get; set; }

        /// <summary>
        /// 临时文件清理超时（分钟）
        /// </summary>
        public int TempFileExpireMinutes { get; set; }
        #endregion

        #region 5. 雪花算法配置
        /// <summary>
        /// 雪花算法WorkerId
        /// </summary>
        public long SnowflakeWorkerId { get; set; }

        /// <summary>
        /// 雪花算法DataCenterId
        /// </summary>
        public long SnowflakeDatacenterId { get; set; }
        #endregion

        #region 6. AI服务配置
        /// <summary>
        /// AI服务接口地址
        /// </summary>
        public string AIService { get; set; }

        /// <summary>
        /// AI请求超时时间（秒）
        /// </summary>
        public int AITimeout { get; set; }
        #endregion

        #region 辅助方法：转换配置值（处理空值/类型转换）
        /// <summary>
        /// 从配置值转换为当前实体
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="value">配置值</param>
        internal void SetValue(string key, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            try
            {
                switch (key.ToLower().Trim())
                {
                    // 系统基础配置
                    case "loginmultiple":
                    case "system:loginmultiple":
                        LoginMultiple = bool.Parse(value);
                        break;
                    case "loginprovider":
                    case "system:loginprovider":
                        LoginProvider = value.Trim();
                        break;
                    case "apisite":
                    case "system:apisite":
                        ApiSite = value.Trim();
                        break;
                    case "allowcorssite":
                    case "system:allowcorssite":
                        AllowCorsSite = value.Trim();
                        break;
                    case "virtualdirectory":
                    case "system:virtualdirectory":
                        VirtualDirectory = value.Trim();
                        break;
                    case "concurrencynumber":
                    case "system:concurrencynumber":
                        ConcurrencyNumber = int.Parse(value);
                        break;
                    case "httptimeout":
                    case "system:httptimeout":
                        HttpTimeout = int.Parse(value);
                        break;

                    // 数据库配置
                    case "dbprovider":
                    case "database:provider":
                        DBProvider = value.Trim();
                        break;
                    case "dbconnectionstring":
                    case "database:connectionstring":
                        DBConnectionString = value.Trim();
                        break;
                    case "dbcommandtimeout":
                    case "database:commandtimeout":
                        DBCommandTimeout = int.Parse(value);
                        break;
                    case "dbslowsqllogtime":
                    case "database:slowsqllogtime":
                        DBSlowSqlLogTime = int.Parse(value);
                        break;
                    case "dbbackuppath":
                    case "database:backuppath":
                        DbBackupPath = value.Trim();
                        break;

                    // MVC基础配置
                    case "webpages:version":
                        WebpagesVersion = value.Trim();
                        break;
                    case "webpages:enabled":
                        WebpagesEnabled = bool.Parse(value);
                        break;
                    case "clientvalidationenabled":
                    case "validation:clientenabled":
                        ClientValidationEnabled = bool.Parse(value);
                        break;
                    case "unobtrusivejavascriptenabled":
                    case "validation:unobtrusiveenabled":
                        UnobtrusiveJavaScriptEnabled = bool.Parse(value);
                        break;

                    // 缓存配置
                    case "cacheprovider":
                    case "cache:provider":
                        CacheProvider = value.Trim();
                        break;
                    case "redisconnectionstring":
                    case "cache:redis:connectionstring":
                        RedisConnectionString = value.Trim();
                        break;

                    // 文件上传配置
                    case "uploadfilebyimgsize":
                    case "upload:imagesize":
                        UploadFileByImgSize = int.Parse(value);
                        break;
                    case "uploadfilebyimgtype":
                    case "upload:imagetype":
                        UploadFileByImgType = value.Trim();
                        break;
                    case "uploadmaxfilesize":
                    case "upload:maxfilesize":
                        UploadMaxFileSize = long.Parse(value);
                        break;
                    case "compressedfileformats":
                    case "upload:compressedformats":
                        CompressedFileFormats = value.Trim();
                        break;
                    case "imagefileformats":
                    case "upload:imageformats":
                        ImageFileFormats = value.Trim();
                        break;

                    // 临时文件配置
                    case "temprootpath":
                    case "temp:rootpath":
                        TempRootPath = value.Trim();
                        break;
                    case "archiverootpath":
                    case "archive:rootpath":
                        ArchiveRootPath = value.Trim();
                        break;
                    case "exportrootpath":
                    case "export:rootpath":
                        ExportRootPath = value.Trim();
                        break;
                    case "tempfileexpireminutes":
                    case "temp:expireminutes":
                        TempFileExpireMinutes = int.Parse(value);
                        break;

                    // 雪花算法配置
                    case "snowflake.workerid":
                    case "snowflake:workerid":
                        SnowflakeWorkerId = long.Parse(value);
                        break;
                    case "snowflake.datacenterid":
                    case "snowflake:datacenterid":
                        SnowflakeDatacenterId = long.Parse(value);
                        break;

                    // AI服务配置
                    case "aiservice":
                    case "ai:service":
                        AIService = value.Trim();
                        break;
                    case "aitimeout":
                    case "ai:timeout":
                        AITimeout = int.Parse(value);
                        break;

                    default:
                        // 可以选择记录未处理的配置项
                        // Logger.Warn($"未处理的配置项: {key} = {value}");
                        break;
                }
            }
            catch (Exception)
            {
                // 建议记录转换异常，但不要抛出以免影响其他配置加载
                // Logger.Error($"配置项 '{key}' 转换失败: {ex.Message}");
                // 可以选择使用默认值或者忽略
                value.ParseToString();
            }
        }
        #endregion
    }
}