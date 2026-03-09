using System.Collections.Generic;
using Cappuccino.Entity;
using Cappuccino.Model;

namespace Cappuccino.IBLL
{
    public interface ISysDepartmentService : IBaseService<SysDepartmentEntity>
    {
        /// <summary>
        /// 获取部门树
        /// </summary>
        /// <returns></returns>
        List<DtreeData> GetDepartmentTree();

        /// <summary>
        /// 获取最大排序码
        /// </summary>
        /// <returns></returns>
        int GetMaxSortCode();

        /// <summary>
        /// 获取dtree数据格式的权限
        /// </summary>
        /// <returns></returns>
        List<DtreeData> GetDepartmentDtree(long roleId);
    }
}
