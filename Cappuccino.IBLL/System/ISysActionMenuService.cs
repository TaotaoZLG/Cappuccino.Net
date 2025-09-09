using System.Collections.Generic;
using Cappuccino.Entity;
using Cappuccino.Model;

namespace Cappuccino.IBLL
{
    public interface ISysActionMenuService : IBaseService<SysActionMenuEntity>
    {
        /// <summary>
        /// 根据用户Id，拿到所拥有的菜单
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<PearMenuViewModel> GetMenu(int userId);
    }
}
