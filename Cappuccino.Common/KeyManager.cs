namespace Cappuccino.Common
{
    public class KeyManager
    {
        public const string UserInfo = "Cappuccino@UserInfo";

        /// <summary>
        /// 用于存放登录成功后用户的id的cookie Key
        /// </summary>
        public const string IsMember = "Cappuccino@IsMember";

        /// <summary>
        /// 用于缓存整个Autofac容器对象的缓存key
        /// </summary>
        public const string AutofacContainer = "Cappuccino@AutofacContainer";

        /// <summary>
        /// 用于缓存IP黑名单的key
        /// </summary>
        public const string IpBlackCacheKey = "Cappuccino@IpBlackList";
    }
}
