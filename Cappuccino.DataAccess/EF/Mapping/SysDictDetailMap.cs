using System.Data.Entity.ModelConfiguration;
using Cappuccino.Entity;

namespace Cappuccino.DataAccess.Mapping
{
    public class SysDictDetailMap : EntityTypeConfiguration<SysDictDetailEntity>
    {
        public SysDictDetailMap()
        {
            this.ToTable("SysDictDetail");
            this.HasKey(x => x.Id);
            this.HasRequired(x => x.SysDict).WithMany(x => x.SysDictDetails).HasForeignKey(x => x.TypeId);

            this.Property(x => x.Code).HasMaxLength(50).IsRequired();
            this.Property(x => x.Name).HasMaxLength(50).IsRequired();
        }
    }
}
