namespace Cappuccino.Model
{
    public class SysActionButtonViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// 按钮编码
        /// </summary>
        public string ButtonCode { get; set; }

        /// <summary>
        /// 位置 0：表内 1：表外
        /// </summary>
        public int Location { get; set; }

        /// <summary>
        /// 按钮样式
        /// </summary>
        public string ButtonClass { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string ButtonIcon { get; set; }

        public SysActionViewModel SysAction { get; set; }

    }
}
