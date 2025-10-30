using System.Collections.Generic;
using Cappuccino.Entity;
using Cappuccino.Model;

namespace Cappuccino.IBLL
{
    public interface ISysRoleService : IBaseService<SysRoleEntity>
    {
        /// <summary>
        /// 根据权限Ids 构建角色权限中间关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dtrees"></param>
        /// <returns></returns>
        void Add(int id, List<DtreeResponse> dtrees);

        /// <summary>
        /// 保存角色的菜单权限和数据权限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="menuPermissions">菜单权限ID列表（原dtree数据）</param>
        /// <param name="dataPermissions">数据权限列表</param>
        void SavePermissions(int roleId, List<DtreeResponse> menuPermissions, List<DtreeResponse> dataPermissions);
    }
}
