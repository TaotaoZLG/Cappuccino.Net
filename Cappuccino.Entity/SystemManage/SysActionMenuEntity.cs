namespace Cappuccino.Entity
{
    public class SysActionMenuEntity
    {
        public int Id { get; set; }

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
