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
    }
}
