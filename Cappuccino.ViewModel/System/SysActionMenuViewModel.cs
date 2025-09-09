namespace Cappuccino.Model
{
    public class SysActionMenuViewModel
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

        public SysActionViewModel SysAction { get; set; }
    }
}
