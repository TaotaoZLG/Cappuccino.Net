using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Cappuccino.Entity;

namespace Cappuccino.DataAccess.Mapping
{
    public class SysActionButtonMap : EntityTypeConfiguration<SysActionButtonEntity>
    {
        public SysActionButtonMap()
        {
            ToTable("SysActionButton");
            HasKey(x => x.Id);
            Property(x => x.Id).HasColumnName("ActionId").HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(x => x.ButtonCode).HasMaxLength(50).IsRequired();
            this.Property(x => x.ButtonClass).HasMaxLength(50).IsRequired();
            this.Property(x => x.ButtonIcon).HasMaxLength(50).IsRequired();

        }
    }
}
