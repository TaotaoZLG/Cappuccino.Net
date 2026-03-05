using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Cappuccino.Entity
{
    [Table("SysDepartment")]
    public class SysDepartmentEntity : BaseEntity
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

        // 反向导航属性（部门包含的用户）
        [JsonIgnore]
        public virtual ICollection<SysUserEntity> SysUsers { get; set; } = new List<SysUserEntity>();
    }
}
