using System.Collections.Generic;
using System.Linq;
using Cappuccino.Common;
using Cappuccino.Common.Caching;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Util;
using Cappuccino.Entity;

namespace Cappuccino.Web.Core
{
    /// <summary>
    /// 负责管理用户的相关操作的
    /// </summary>
    public class UserManager
    {
        /// <summary>
        /// 负责获取当前登录用户的实体对象
        /// </summary>
        /// <returns></returns>
        public static SysUserEntity GetCurrentUserInfo()
        {
            var cacheId = GetCurrentUserCacheId();
            if (string.IsNullOrEmpty(cacheId))
            {
                return null;
            }
            var userEntity = CacheManager.Cache.Get<SysUserEntity>(cacheId);
            return userEntity ?? null;
        }

        /// <summary>
        /// 获取当前登录用户缓存信息的Key
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentUserCacheId()
        {
            string cookieValue = CookieHelper.Get(KeyManager.IsMember);
            if (string.IsNullOrEmpty(cookieValue))
            {
                return "";
            }
            List<string> list = DESUtils.Decrypt(cookieValue).ToList<string>();
            return (list != null && list.Count == 2) ? list[0] : "";
        }
    }
}
