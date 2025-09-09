using System.Data.Entity.ModelConfiguration;
using Cappuccino.Entity;

namespace Cappuccino.DAL.Mapping
{
    public class SysDictTypeMap : EntityTypeConfiguration<SysDictTypeEntity>
    {
        public SysDictTypeMap()
        {
            this.ToTable("SysDictType");
            this.HasKey(x => x.Id);
            this.Property(x => x.Code).HasMaxLength(50).IsRequired();
            this.Property(x => x.Name).HasMaxLength(50).IsRequired();
            this.Property(x => x.SortCode);
        }
    }
}
