using System;
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
        ISysActionMenuDao _actionMenuDao;
        ISysActionService SysActionService;
        public SysActionMenuService(ISysActionMenuDao actionMenuDao, ISysActionService sysActionService)
        {
            _actionMenuDao = actionMenuDao;
            base.CurrentDao = actionMenuDao;
            SysActionService = sysActionService;
            this.AddDisposableObject(this.CurrentDao);
            this.AddDisposableObject(this.SysActionService);
        }
        #endregion

        public List<PearMenuModel> GetMenu(int userId)
        {
            var sysActionList = SysActionService.GetPermissionByType(userId, ActionTypeEnum.Menu)
                .Concat(SysActionService.GetPermissionByType(userId, ActionTypeEnum.Directory))
                .OrderBy(x => x.SortCode)
                .ToList();
            //var sysActionMenus = _actionMenuDao.GetList(x => true).ToList();
            var sysActionMenus = _actionMenuDao.GetList(x => true).ToDictionary(x => x.Id);

            if (sysActionList == null)
            {
                return new List<PearMenuModel>();
            }

            List<PearMenuModel> allMenuList = new List<PearMenuModel>();
            Dictionary<int, PearMenuModel> menuDict = new Dictionary<int, PearMenuModel>();
            foreach (var item in sysActionList)
            {
                PearMenuModel pearMenuData = new PearMenuModel();
                pearMenuData.Id = item.Id;
                pearMenuData.Title = item.Name ?? string.Empty;
                pearMenuData.ParentId = item.ParentId;
                pearMenuData.Children = new List<PearMenuModel>();  //初始化子菜单列表
                pearMenuData.Href = item.SysActionMenu?.Url ?? "javascript:;";
                pearMenuData.OpenType = string.Empty;

                if (item.Type == ActionTypeEnum.Directory)
                {
                    pearMenuData.Type = 2;
                    pearMenuData.Icon = string.IsNullOrEmpty(item.SysActionMenu?.Icon) ? "layui-icon layui-icon-menu-fill" : "layui-icon " + item.SysActionMenu.Icon;
                }
                else if (item.Type == ActionTypeEnum.Menu)
                {
                    pearMenuData.Type = 0;
                    pearMenuData.OpenType = "_iframe"; // 菜单默认iframe跳转
                    pearMenuData.Icon = string.IsNullOrEmpty(item.SysActionMenu?.Icon) ? "layui-icon layui-icon-app" : "layui-icon " + item.SysActionMenu.Icon;
                }

                allMenuList.Add(pearMenuData);
                menuDict.Add(item.Id, pearMenuData);
            }

            List<PearMenuModel> rootMenuList = new List<PearMenuModel>();
            foreach (var item in allMenuList)
            {
                if (item.ParentId == 0)
                {
                    // 顶级节点（ParentId=0）：无论是目录还是菜单，都加入根列表
                    rootMenuList.Add(item);
                }
                else if (menuDict.TryGetValue(item.ParentId, out var parentMenu))
                {
                    //TryGetValue替代ContainsKey+索引，减少字典查询次数
                    // 父节点（无论目录/菜单）若有子节点，设为折叠类型（Type=0）
                    parentMenu.Children.Add(item);
                    parentMenu.Type = 0;
                    parentMenu.OpenType = ""; // 父节点不跳转，子节点跳转
                }

                if (item.Type == (int)ActionTypeEnum.Menu && !item.Children.Any())
                {
                    item.Type = 1;
                    item.OpenType = "_iframe";
                }
            }

            return rootMenuList;
        }
    }
}
