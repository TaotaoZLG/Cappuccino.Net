using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Cappuccino.Common.Enum;

namespace Cappuccino.Entity
{
    public class SysActionEntity : BaseEntity
    {

        /// <summary>
        /// 权限名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 权限标识
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 上级
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// 权限类型 0为菜单 1为按钮 2为目录
        /// </summary>
        public ActionTypeEnum Type { get; set; }

        /// <summary>
        /// 排序码
        /// </summary>
        public int SortCode { get; set; }

        public SysActionMenuEntity SysActionMenu { get; set; }

        public SysActionButtonEntity SysActionButton { get; set; }

        public virtual ICollection<SysRoleEntity> SysRoles { get; set; } = new List<SysRoleEntity>();

        public virtual ICollection<SysUserActionEntity> SysUserActions { get; set; } = new List<SysUserActionEntity>();
    }
}
