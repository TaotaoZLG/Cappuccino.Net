using System.Collections.Generic;
using Cappuccino.Entity;

namespace Cappuccino.IDAL
{
    public interface ISysDataAuthorizeDao : IBaseDao<SysDataAuthorizeEntity>
    {
        // 可扩展：根据角色ID删除旧数据权限
        void DeleteByRoleId(long roleId);

        // 获取全部数据权限列表
        HashSet<SysDataAuthorizeEntity> GetAuthorizeList();
    }
}
