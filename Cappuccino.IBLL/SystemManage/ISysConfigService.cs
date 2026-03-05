using Cappuccino.Entity;
namespace Cappuccino.IBLL
{
    public interface ISysConfigService : IBaseService<SysConfigEntity>
    {
        SysConfigEntity GetByConfig(string configKey);
    }
}
