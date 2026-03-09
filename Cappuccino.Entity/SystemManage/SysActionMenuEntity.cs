namespace Cappuccino.Entity
{
    public class SysActionMenuEntity : BaseField
    {
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        public string Url { get; set; }

        public SysActionEntity SysAction { get; set; }
    }
}
