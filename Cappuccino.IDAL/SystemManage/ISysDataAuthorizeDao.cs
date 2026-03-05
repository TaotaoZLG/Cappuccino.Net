using Cappuccino.Entity;

namespace Cappuccino.IDAL
{
    public interface ISysDataAuthorizeDao : IBaseDao<SysDataAuthorizeEntity>
    {
        // 可扩展：根据角色ID删除旧数据权限
        void DeleteByRoleId(int roleId);
    }
}
