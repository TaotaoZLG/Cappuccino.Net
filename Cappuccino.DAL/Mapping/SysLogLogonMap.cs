using System.Data.Entity.ModelConfiguration;
using Cappuccino.Entity;

namespace Cappuccino.DAL.Mapping
{
    public class SysLogLogonMap : EntityTypeConfiguration<SysLogLogonEntity>
    {
        public SysLogLogonMap()
        {
            this.ToTable("SysLogLogon");
            this.HasKey(x => x.Id);
            this.Property(x => x.LogType).HasMaxLength(50).IsRequired();
            this.Property(x => x.Account).HasMaxLength(50).IsRequired();
            this.Property(x => x.RealName).HasMaxLength(50).IsRequired();
            this.Property(x => x.Description).HasMaxLength(200).IsRequired();
            this.Property(x => x.IPAddress).HasMaxLength(50).IsRequired();
            this.Property(x => x.IPAddressName).HasMaxLength(50).IsRequired();
        }
    }
}
