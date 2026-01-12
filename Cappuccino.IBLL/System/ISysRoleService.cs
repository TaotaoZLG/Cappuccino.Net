using System.Collections.Generic;
using Cappuccino.Entity;
using Cappuccino.Model;

namespace Cappuccino.IBLL
{
    public interface ISysRoleService : IBaseService<SysRoleEntity>
    {
        /// <summary>
        /// 保存角色菜单权限
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="menuPermissions"></param>
        /// <returns></returns>
        void SaveMenuPermissions(SysRoleEntity roleEntity, List<DtreeResponse> menuPermissions);

        /// <summary>
        /// 保存角色数据权限
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="dataPermissions"></param>
        /// <returns></returns>
        void SaveDataPermissions(int roleId, List<DtreeResponse> dataPermissions);

        /// <summary>
        /// 保存角色的菜单权限和数据权限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="menuPermissions">菜单权限列表</param>
        /// <param name="dataPermissions">数据权限列表</param>
        void SavePermissions(int roleId, List<DtreeResponse> menuPermissions, List<DtreeResponse> dataPermissions);
    }
}
