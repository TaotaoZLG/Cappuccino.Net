namespace Cappuccino.Model
{
    public class SysUserActionModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public virtual SysUserModel SysUser { get; set; }
        public long ActionId { get; set; }
        public virtual SysActionModel SysAction { get; set; }
        public bool HasPermisssin { get; set; }

    }
}
