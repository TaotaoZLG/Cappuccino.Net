using System.Collections.Generic;
using System.Linq;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;
using Cappuccino.Model;

namespace Cappuccino.BLL
{
    public class SysUserActionService : BaseService<SysUserActionEntity>, ISysUserActionService
    {
        #region 依赖注入
        readonly ISysUserActionDao dao;
        readonly ISysActionDao SysActionDao;
        public SysUserActionService(ISysUserActionDao dao, ISysActionDao sysActionDao)
        {
            this.dao = dao;
            base.CurrentDao = dao;
            SysActionDao = sysActionDao;
            this.AddDisposableObject(this.CurrentDao);
            this.AddDisposableObject(SysActionDao);
        }
        #endregion

        public List<UserActionModel> GetUserActionList(int userId)
        {
            List<UserActionModel> userActions = new List<UserActionModel>();
            var actions = SysActionDao.GetList(x => true).OrderBy(x => x.SortCode).ToList();
            var myUserActions = GetList(x => x.UserId == userId).ToList();
            foreach (var item in actions)
            {
                UserActionModel viewModel = new UserActionModel();
                viewModel.Id = item.Id;
                viewModel.ParentId = item.ParentId;
                viewModel.Code = item.Code;
                viewModel.Name = item.Name;
                var myUserAction = myUserActions.Where(x => x.ActionId == item.Id).FirstOrDefault();
                if (myUserAction != null)
                {
                    if (myUserAction.HasPermisssin)
                    {
                        viewModel.Status = 1;
                    }
                    else
                    {
                        viewModel.Status = 2;
                    }
                }
                userActions.Add(viewModel);
            }
            return userActions;
        }

        public bool SaveUserAction(int userId, List<UserActionModel> userActions)
        {
            userActions = userActions.Where(x => x.Status != 0).ToList();
            if (userActions.Count == 0)
            {
                return true;
            }
            dao.DeleteBy(x => x.UserId == userId);
            foreach (var item in userActions)
            {
                SysUserActionEntity userAction = new SysUserActionEntity();
                userAction.UserId = userId;
                userAction.ActionId = item.Id;
                if (item.Status == 1)
                {
                    userAction.HasPermisssin = true;
                }
                else if (item.Status == 2)
                {
                    userAction.HasPermisssin = false;
                }
                dao.Insert(userAction);
            }
            return dao.SaveChanges() > 0;
        }
    }
}
