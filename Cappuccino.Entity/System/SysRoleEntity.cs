using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cappuccino.Entity
{
    public class SysRoleEntity : BaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int EnabledMark { get; set; }
        public string Remark { get; set; }

        public virtual ICollection<SysUserEntity> SysUsers { get; set; } = new List<SysUserEntity>();
        public virtual ICollection<SysActionEntity> SysActions { get; set; } = new List<SysActionEntity>();
    }
}
