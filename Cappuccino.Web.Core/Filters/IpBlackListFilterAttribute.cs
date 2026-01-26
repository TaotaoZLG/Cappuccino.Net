using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Autofac;
using Cappuccino.Common;
using Cappuccino.Common.Caching;
using Cappuccino.Common.Net;
using Cappuccino.IBLL;

namespace Cappuccino.Web.Core.Filters
{
    /// <summary>
    /// IP黑名单过滤器
    /// </summary>
    public class IpBlackListFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 1. 获取客户端真实IP地址
            var clientIp = NetHelper.GetIp;

            // 2. 从缓存获取IP黑名单列表
            var blackList = GetIpBlackListFromCache();

            // 3. 检查IP是否在黑名单中
            // 如果黑名单为空，则不进行任何拦截
            if (blackList != null && blackList.Any() && blackList.Contains(clientIp))
            {
                // IP在黑名单中，拒绝访问
                filterContext.Result = new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden,
                    $"Access denied. Your IP {clientIp} is blacklisted."
                );
                return; // 中断后续执行
            }

            // IP不在黑名单中，或黑名单为空，继续执行后续操作
            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// 从数据库获取IP黑名单
        /// </summary>
        private List<string> GetIpBlackListFromCache()
        {
            // 从缓存获取
            var blackList = CacheManager.Get<List<string>>(KeyManager.IpBlackCacheKey);
            if (blackList != null)
            {
                return blackList;
            }

            // 缓存不存在则从数据库读取
            var container = CacheManager.Get<IContainer>(KeyManager.AutofacContainer);
            var _sysConfigService = container.Resolve<ISysConfigService>();

            var configEntity = _sysConfigService.GetByConfig("sys_ipBlackList");
            if (configEntity != null && !string.IsNullOrWhiteSpace(configEntity.ConfigValue))
            {
                blackList = configEntity.ConfigValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(ip => ip.Trim()).ToList();
            }
            else
            {
                blackList = new List<string>();
            }

            // 存入缓存（永不过期）
            CacheManager.Set(KeyManager.IpBlackCacheKey, blackList);

            return blackList;
        }
    }
}