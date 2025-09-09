using System.Collections.Generic;
using Cappuccino.Common.Enum;
using Cappuccino.Entity;
using Cappuccino.Model;

namespace Cappuccino.IBLL
{
    public interface ISysActionButtonService : IBaseService<SysActionButtonEntity>
    {
        /// <summary>
        /// 根据用户和菜单按钮位置获得按钮列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="menuId"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        List<ButtonViewModel> GetButtonListByUserIdAndMenuId(int userId, string url, PositionEnum position);
    }
}
