namespace Cappuccino.Model
{
    public class SysActionMenuModel
    {
        public long Id { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        public string Url { get; set; }

        public SysActionModel SysAction { get; set; }
    }
}
