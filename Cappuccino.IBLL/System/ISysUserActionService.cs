using System.Collections.Generic;
using Cappuccino.Entity;
using Cappuccino.Model.System;

namespace Cappuccino.IBLL
{
    public interface ISysUserActionService : IBaseService<SysUserActionEntity>
    {
        /// <summary>
        /// 获取用户特权
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<UserActionViewModel> GetUserActionList(int userId);

        /// <summary>
        /// 更新用户特殊权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userActions"></param>
        bool SaveUserAction(int userId, List<UserActionViewModel> userActions);
    }
}
