using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.DataAccess;
using Cappuccino.Entity;
using Cappuccino.IDAL;

namespace Cappuccino.DAL
{
    public class SysDataAuthorizeDao : BaseDao<SysDataAuthorizeEntity>, ISysDataAuthorizeDao
    {
        // 根据角色ID删除数据权限（AuthorizeId=角色ID）
        public void DeleteByRoleId(int roleId)
        {
            var entities = GetList(x => x.AuthorizeId == roleId);

            if (entities.Count() > 0)
            {
                Delete(entities);
            }
        }
    }
}
