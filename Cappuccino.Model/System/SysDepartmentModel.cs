using System.Collections.Generic;

namespace Cappuccino.Model
{
    /// <summary>
    /// 部门
    /// </summary>
    public class SysDepartmentModel : BaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 父部门Id(0表示是根部门)
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// 排序码
        /// </summary>
        public int SortCode { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
