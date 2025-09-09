using System.Collections.Generic;
using Cappuccino.Common.Enum;

namespace Cappuccino.Model
{
    public class SysActionViewModel : BaseEntity
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
        /// 权限类型 0为菜单 1为功能
        /// </summary>
        public ActionTypeEnum Type { get; set; }

        /// <summary>
        /// 排序码
        /// </summary>
        public int SortCode { get; set; }

        public SysActionMenuViewModel SysActionMenu { get; set; }

        public SysActionButtonViewModel SysActionButton { get; set; }

        public virtual ICollection<SysRoleViewModel> SysRoles { get; set; }

        public virtual ICollection<SysUserActionViewModel> SysUserActions { get; set; }
    }
}
