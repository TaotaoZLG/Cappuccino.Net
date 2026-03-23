using Cappuccino.Entity;
namespace Cappuccino.IDAL
{
    public interface ISysConfigDao : IBaseDao<SysConfigEntity>
    {
        /// <summary>
        /// 可扩展根据键名查询等方法
        /// </summary>
        /// <param name="configKey"></param>
        /// <returns></returns>
        SysConfigEntity GetConfigByKey(string configKey);
    }
}
