using System.Collections.Generic;

namespace Cappuccino.Model
{
    public class SysUserModel : BaseEntity
    {
        public string UserName { get; set; }
        public string NickName { get; set; }
        public int? DepartmentId { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string HeadIcon { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
        public int? UserStatus { get; set; }
        public string RoleIds { get; set; }
        public virtual ICollection<SysRoleModel> SysRoles { get; set; }
        public virtual ICollection<SysUserActionModel> SysUserActions { get; set; }

    }
}
