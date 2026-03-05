using System.Collections.Generic;
using System.Linq;
using Cappuccino.Common.Enum;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;
using Cappuccino.Model;

namespace Cappuccino.BLL
{
    public class SysActionButtonService : BaseService<SysActionButtonEntity>, ISysActionButtonService
    {
        #region 依赖注入
        ISysActionButtonDao _actionButtonDao;
        ISysActionService _actionService;
        ISysActionMenuDao _actionMenuDao;
        public SysActionButtonService(ISysActionButtonDao actionButtonDao, ISysActionService actionService, ISysActionMenuDao actionMenuDao)
        {
            _actionButtonDao = actionButtonDao;
            base.CurrentDao = actionButtonDao;
            _actionService = actionService;
            _actionMenuDao = actionMenuDao;
            this.AddDisposableObject(this.CurrentDao);
            this.AddDisposableObject(_actionService);
            this.AddDisposableObject(_actionMenuDao);
        }
        #endregion

        public List<ButtonModel> GetButtonListByUserIdAndMenuId(int userId, string url, PositionEnum position)
        {
            List<ButtonModel> buttonModelList = new List<ButtonModel>();
            var sysActionButtons = _actionButtonDao.GetList(x => true).ToList();
            var menu = _actionMenuDao.GetList(x => x.Url == url).FirstOrDefault();
            if (menu == null)
            {
                return buttonModelList;
            }
            var sysActionList = _actionService.GetPermissionByType(userId, ActionTypeEnum.Button)
                .Where(x => x.ParentId == menu.Id && x.SysActionButton.Location == position).OrderBy(x => x.SortCode).ToList();
            foreach (var item in sysActionList)
            {
                ButtonModel buttonModel = new ButtonModel();
                buttonModel.FullName = item.Name;
                buttonModel.ButtonCode = item.SysActionButton.ButtonCode;
                buttonModel.ClassName = item.SysActionButton.ButtonClass;
                buttonModel.Icon = item.SysActionButton.ButtonIcon;
                buttonModelList.Add(buttonModel);
            }
            return buttonModelList;
        }
    }
}
