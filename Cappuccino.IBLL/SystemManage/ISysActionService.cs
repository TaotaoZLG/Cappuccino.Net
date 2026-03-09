using System.Collections.Generic;
using Cappuccino.Common.Enum;
using Cappuccino.Entity;
using Cappuccino.Model;

namespace Cappuccino.IBLL
{
    public interface ISysActionService : IBaseService<SysActionEntity>
    {
        /// <summary>
        /// 根据用户获取所拥有的权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        List<SysActionEntity> GetPermission(long userId);

        /// <summary>
        /// 根据用户获取所拥有的菜单或按钮
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="type">权限类型</param>
        /// <returns></returns>
        List<SysActionEntity> GetPermissionByType(long userId, ActionTypeEnum type);

        /// <summary>
        /// 获取dtree数据格式的权限
        /// </summary>
        /// <returns></returns>
        List<DtreeData> GetDtree(long roleId);

        /// <summary>
        /// 是否包含该权限
        /// </summary>
        /// <param name="id"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        bool HasPermission(long id, string permission);

        /// <summary>
        /// 获取菜单树
        /// </summary>
        /// <returns></returns>
        List<DtreeData> GetMenuTree();

        /// <summary>
        /// 批量删除菜单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        bool DeleteByIds(long[] ids);
    }
}
