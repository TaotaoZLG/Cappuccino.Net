using System.Data.Entity.ModelConfiguration;
using Cappuccino.Entity;

namespace Cappuccino.DAL.Mapping
{
    public class SysDictMap : EntityTypeConfiguration<SysDictEntity>
    {
        public SysDictMap()
        {
            this.ToTable("SysDict");
            this.HasKey(x => x.Id);
            this.HasRequired(x => x.SysDictType).WithMany(x => x.SysDicts).HasForeignKey(x => x.TypeId);

            this.Property(x => x.Code).HasMaxLength(50).IsRequired();
            this.Property(x => x.Name).HasMaxLength(50).IsRequired();
        }
    }
}
