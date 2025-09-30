using System.Collections.Generic;

namespace Cappuccino.Model
{
    /// <summary>
    /// 数据字典
    /// </summary>
    public class SysDictDetailModel : BaseEntity
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
        /// 分类主键
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// 排序码
        /// </summary>
        public int SortCode { get; set; }

        public virtual SysDictModel SysDict { get; set; }
    }
}
