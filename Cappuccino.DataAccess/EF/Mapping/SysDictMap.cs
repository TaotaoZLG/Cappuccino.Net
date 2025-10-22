using System.Data.Entity.ModelConfiguration;
using Cappuccino.Entity;

namespace Cappuccino.DataAccess.Mapping
{
    public class SysDictMap : EntityTypeConfiguration<SysDictEntity>
    {
        public SysDictMap()
        {
            this.ToTable("SysDict");
            this.HasKey(x => x.Id);
            this.Property(x => x.Code).HasMaxLength(50).IsRequired();
            this.Property(x => x.Name).HasMaxLength(50).IsRequired();
            this.Property(x => x.SortCode);
        }
    }
}
