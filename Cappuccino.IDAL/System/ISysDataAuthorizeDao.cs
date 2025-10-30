using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Entity;

namespace Cappuccino.IDAL
{
    public interface ISysDataAuthorizeDao : IBaseDao<SysDataAuthorizeEntity>
    {
        // 可扩展：根据角色ID删除旧数据权限
        void DeleteByRoleId(int roleId);
    }
}
