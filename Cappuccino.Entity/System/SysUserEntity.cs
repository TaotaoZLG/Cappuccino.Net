using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cappuccino.Entity
{
    public class SysUserEntity : BaseEntity
    {
        public string UserName { get; set; }
        public string NickName { get; set; }
        public int? DepartmentId { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string HeadIcon { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
        public int UserStatus { get; set; }
        public int IsSystem { get; set; }

        [NotMapped]
        public string DepartmentName { get; set; }
        [NotMapped]
        public string RoleName { get; set; }

        // 用户角色
        public virtual ICollection<SysRoleEntity> SysRoles { get; set; } = new List<SysRoleEntity>();
        // 用户自定义权限
        public virtual ICollection<SysUserActionEntity> SysUserActions { get; set; } = new List<SysUserActionEntity>();

        [ForeignKey("DepartmentId")]  // 指定导航属性对应的外键
        public virtual SysDepartmentEntity Department { get; set; }

    }
}
