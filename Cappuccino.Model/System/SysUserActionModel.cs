namespace Cappuccino.Model
{
    public class SysUserActionModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual SysUserModel SysUser { get; set; }
        public int ActionId { get; set; }
        public virtual SysActionViewModel SysAction { get; set; }
        public bool HasPermisssin { get; set; }

    }
}
