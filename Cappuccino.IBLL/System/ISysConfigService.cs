using Cappuccino.Entity;
namespace Cappuccino.IBLL
{
    public interface ISysConfigService : IBaseService<SysConfigEntity>
    {
        SysConfigEntity GetByKey(string key);
    }
}
