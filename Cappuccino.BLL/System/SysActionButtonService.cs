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
        ISysActionButtonDao dao;
        ISysActionService SysActionService;
        ISysActionMenuDao SysActionMenuDao;
        public SysActionButtonService(ISysActionButtonDao dao, ISysActionService sysActionService, ISysActionMenuDao sysActionMenuDao)
        {
            this.dao = dao;
            base.CurrentDao = dao;
            SysActionService = sysActionService;
            SysActionMenuDao = sysActionMenuDao;
            this.AddDisposableObject(this.CurrentDao);
            this.AddDisposableObject(this.SysActionService);
            this.AddDisposableObject(this.SysActionMenuDao);
        }
        #endregion

        public List<ButtonModel> GetButtonListByUserIdAndMenuId(int userId, string url, PositionEnum position)
        {
            List<ButtonModel> buttonModelList = new List<ButtonModel>();
            var sysActionButtons = dao.GetList(x => true).ToList();
            var menu = SysActionMenuDao.GetList(x => x.Url == url).FirstOrDefault();
            if (menu == null)
            {
                return buttonModelList;
            }
            var sysActionList = SysActionService.GetPermissionByType(userId, ActionTypeEnum.Button)
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
