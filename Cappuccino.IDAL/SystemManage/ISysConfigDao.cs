using Cappuccino.Entity;
namespace Cappuccino.IDAL
{
    public interface ISysConfigDao : IBaseDao<SysConfigEntity>
    {
        // 可扩展根据键名查询等方法
        SysConfigEntity GetByConfig(string configKey);
    }
}
