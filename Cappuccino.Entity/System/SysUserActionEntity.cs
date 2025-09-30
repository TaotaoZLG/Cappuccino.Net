using System.ComponentModel.DataAnnotations.Schema;

namespace Cappuccino.Entity
{
    public class SysUserActionEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual SysUserEntity SysUser { get; set; }
        public int ActionId { get; set; }
        public virtual SysActionEntity SysAction { get; set; }
        public bool HasPermisssin { get; set; }

    }
}
