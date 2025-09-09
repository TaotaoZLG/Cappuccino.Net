using System.Data.Entity.ModelConfiguration;
using Cappuccino.Entity;

namespace Cappuccino.DAL.Mapping
{
    public class SysUserActionMap : EntityTypeConfiguration<SysUserActionEntity>
    {
        public SysUserActionMap()
        {
            this.ToTable("SysUserAction");
            this.HasKey(x => x.Id);
            this.HasRequired(x => x.SysUser).WithMany(x => x.SysUserActions).HasForeignKey(x => x.UserId);
            this.HasRequired(x => x.SysAction).WithMany(x => x.SysUserActions).HasForeignKey(x => x.ActionId);
        }
    }
}
