using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cappuccino.Entity
{
    public class SysUserEntity : BaseEntity
    {
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string HeadIcon { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
        public int EnabledMark { get; set; }

        public virtual ICollection<SysRoleEntity> SysRoles { get; set; } = new List<SysRoleEntity>();
        public virtual ICollection<SysUserActionEntity> SysUserActions { get; set; } = new List<SysUserActionEntity>();

    }
}
