namespace Cappuccino.Entity
{
    /// <summary>
    /// 登录日志
    /// </summary>
    public class SysLogLogonEntity : BaseCreateEntity
    {
        /// <summary>
        /// 登录类型
        /// </summary>
        public string LogType { get; set; }

        /// <summary>
        /// 账户
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// IP所在城市
        /// </summary>
        public string IPAddressName { get; set; }

        /// <summary>
        /// 操作系统
        /// </summary>
        public string SystemOs { get; set; }

        /// <summary>
        /// 浏览器
        /// </summary>
        public string Browser { get; set; }
    }
}
