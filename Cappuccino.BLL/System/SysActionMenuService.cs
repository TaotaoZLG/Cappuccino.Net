using System.Collections.Generic;
using System.Linq;
using Cappuccino.Common.Enum;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;
using Cappuccino.Model;

namespace Cappuccino.BLL
{
    public class SysActionMenuService : BaseService<SysActionMenuEntity>, ISysActionMenuService
    {
        #region 依赖注入
        ISysActionMenuDao dao;
        ISysActionService SysActionService;
        public SysActionMenuService(ISysActionMenuDao dao, ISysActionService sysActionService)
        {
            this.dao = dao;
            base.CurrentDao = dao;
            SysActionService = sysActionService;
            this.AddDisposableObject(this.CurrentDao);
            this.AddDisposableObject(this.SysActionService);
        }
        #endregion

        public List<PearMenuModel> GetMenu(int userId)
        {
            var sysActionList = SysActionService.GetPermissionByType(userId, ActionTypeEnum.Menu).OrderBy(x => x.SortCode).ToList();
            var sysActionMenus = dao.GetList(x => true).ToList();
            if (sysActionList == null)
            {
                return null;
            }
            //actionMenu转PearMenuData
            List<PearMenuModel> list = new List<PearMenuModel>();
            //返回list
            List<PearMenuModel> pearMenuDatas = new List<PearMenuModel>();
            Dictionary<int, PearMenuModel> dict = new Dictionary<int, PearMenuModel>();
            foreach (var item in sysActionList)
            {
                PearMenuModel pearMenuData = new PearMenuModel();
                pearMenuData.Id = item.Id;
                pearMenuData.Title = item.Name;
                pearMenuData.Icon = "layui-icon " + item.SysActionMenu.Icon;
                pearMenuData.Href = item.SysActionMenu.Url;
                pearMenuData.ParentId = item.ParentId;
                list.Add(pearMenuData);
                dict.Add(item.Id, pearMenuData);
                if (item.ParentId == 0)
                {
                    pearMenuDatas.Add(pearMenuData);
                }
            }
            foreach (var item in list)
            {
                if (item.ParentId != 0 && dict.ContainsKey(item.ParentId))
                {
                    dict[item.ParentId].OpenType = "";
                    dict[item.ParentId].Type = 0;
                    dict[item.ParentId].Children.Add(item);
                    if (dict[item.Id].Children.Count == 0)
                    {
                        item.Type = 1;
                        item.OpenType = "_iframe";
                    }
                    else
                    {
                        item.Type = 0;

                    }
                }
            }
            return pearMenuDatas;
        }

    }
}
