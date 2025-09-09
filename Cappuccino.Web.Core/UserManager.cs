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
            if (!string.IsNullOrEmpty(GetCurrentUserCacheId()))
            {
                return CacheManager.Cache.Get<SysUserEntity>(cacheId);
            }
            return new SysUserEntity() { };
        }

        /// <summary>
        /// 获取当前登录用户缓存信息的Key
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentUserCacheId()
        {
            List<string> list = DESUtils.Decrypt(CookieHelper.Get(KeyManager.IsMember)).ToList<string>();
            if (list != null && list.Count() == 2)
            {
                return list[0];
            }
            return "";
        }
    }
}
