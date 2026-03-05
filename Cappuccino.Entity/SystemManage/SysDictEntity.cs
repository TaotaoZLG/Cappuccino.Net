using System.Collections.Generic;

namespace Cappuccino.Entity
{
    /// <summary>
    /// 字典
    /// </summary>
    public class SysDictEntity : BaseEntity
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
        /// 排序码
        /// </summary>
        public int SortCode { get; set; }

        public virtual ICollection<SysDictDetailEntity> SysDictDetails { get; set; } = new List<SysDictDetailEntity>();
    }
}
