using System.Collections.Generic;
using Cappuccino.Entity;
using Cappuccino.Model;

namespace Cappuccino.IBLL
{
    public interface ISysDataAuthorizeService : IBaseService<SysDataAuthorizeEntity>
    {
        /// <summary>
        /// 根据用户获取数据权限列表
        /// </summary>
        /// <param name="userEntity"></param>
        /// <returns></returns>
        DataAuthorizeInfo GetAuthorizeList(SysUserEntity userEntity = null);
    }
}
