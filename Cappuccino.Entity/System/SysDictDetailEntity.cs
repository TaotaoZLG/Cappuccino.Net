namespace Cappuccino.Entity
{
    /// <summary>
    /// 字典详情
    /// </summary>
    public class SysDictDetailEntity : BaseEntity
    {
        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 显示样式
        /// </summary>
        public string ListClass { get; set; }

        /// <summary>
        /// 字典Id（关联SysDict表Id）
        /// </summary>
        public int DictId { get; set; }

        /// <summary>
        /// 排序码
        /// </summary>
        public int SortCode { get; set; }

        public virtual SysDictEntity SysDict { get; set; }
    }
}
