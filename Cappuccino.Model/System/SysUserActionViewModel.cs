namespace Cappuccino.Model
{
    public class SysUserActionViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual SysUserViewModel SysUser { get; set; }
        public int ActionId { get; set; }
        public virtual SysActionViewModel SysAction { get; set; }
        public bool HasPermisssin { get; set; }

    }
}
