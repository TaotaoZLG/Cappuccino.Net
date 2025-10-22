using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Cappuccino.Entity;

namespace Cappuccino.DataAccess.Mapping
{
    public class SysActionMap : EntityTypeConfiguration<SysActionEntity>
    {
        public SysActionMap()
        {
            ToTable("SysAction");
            HasKey(x => x.Id);
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasOptional(x => x.SysActionMenu).WithRequired(x => x.SysAction);
            HasOptional(x => x.SysActionButton).WithRequired(x => x.SysAction);

            this.Property(x => x.Code).HasMaxLength(50).IsRequired();
            this.Property(x => x.Name).HasMaxLength(50).IsRequired();

        }
    }
}
