namespace Cappuccino.Entity
{
    public class SysUserActionEntity : BaseField
    {
        public long UserId { get; set; }
        public virtual SysUserEntity SysUser { get; set; }
        public long ActionId { get; set; }
        public virtual SysActionEntity SysAction { get; set; }
        public bool HasPermisssin { get; set; }

    }
}
