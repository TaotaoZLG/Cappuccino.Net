using System;
using System.Linq;
using System.Web.Mvc;
using Cappuccino.Common;
using Cappuccino.Common.Enum;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.Model;
using Cappuccino.Web.Core;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysActionController : BaseController
    {
        private readonly ISysActionService _sysActionService;
        private readonly ISysActionButtonService _sysActionButtonService;
        private readonly ISysActionMenuService _sysActionMenuService;

        public SysActionController(ISysActionService sysActionService, ISysActionButtonService sysActionButtonService, ISysActionMenuService sysActionMenuService)
        {
            _sysActionService = sysActionService;
            _sysActionButtonService = sysActionButtonService;
            _sysActionMenuService = sysActionMenuService;
        }

        #region 视图
        [CheckPermission("system.menu.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }

        [HttpGet, CheckPermission("system.menu.create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet, CheckPermission("system.menu.edit")]
        public ActionResult Edit(int id)
        {
            var entity = _sysActionService.GetList(x => x.Id == id).FirstOrDefault();
            _sysActionMenuService.GetList(x => x.Id == id).FirstOrDefault();
            _sysActionButtonService.GetList(x => x.Id == id).FirstOrDefault();
            var viewModel = entity.EntityMap();
            if (viewModel.Type == ActionTypeEnum.Menu)
            {
                return View("EditMenu", viewModel);
            }
            else if (viewModel.Type == ActionTypeEnum.Button)
            {
                return View("EditButton", viewModel);
            }
            else
            {
                return View();
            }
        }
        #endregion

        #region 提交数据
        [HttpPost, CheckPermission("system.menu.create")]
        public ActionResult Create(ActionViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return WriteError("实体验证失败");
                }

                SysActionEntity sysAction = new SysActionEntity
                {
                    Name = viewModel.Name,
                    ParentId = viewModel.ParentId,
                    Code = viewModel.Code,
                    Type = viewModel.Type,
                    SortCode = viewModel.SortCode,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    CreateUserId = UserManager.GetCurrentUserInfo().Id,
                    UpdateUserId = UserManager.GetCurrentUserInfo().Id
                };

                if (viewModel.Type == ActionTypeEnum.Menu)
                {
                    sysAction.SysActionMenu = new SysActionMenuEntity { Icon = viewModel.Icon, Url = viewModel.Url };
                }
                else if (viewModel.Type == ActionTypeEnum.Button)
                {
                    sysAction.SysActionButton = new SysActionButtonEntity
                    {
                        ButtonCode = viewModel.ButtonCode,
                        Location = viewModel.Location,
                        ButtonClass = viewModel.ButtonClass,
                        ButtonIcon = viewModel.ButtonIcon
                    };
                }
                else
                {
                    return WriteError("类型不正确");
                }
                _sysActionService.Add(sysAction);
                return WriteSuccess();
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }



        [HttpPost, CheckPermission("system.menu.edit")]
        public ActionResult Edit(ActionViewModel viewModel)
        {
            if (ModelState.IsValid == false)
            {
                return WriteError("实体验证失败");
            }
            var action = _sysActionService.GetList(x => x.Id == viewModel.Id).FirstOrDefault();
            _sysActionMenuService.GetList(x => x.Id == viewModel.Id).FirstOrDefault();
            _sysActionButtonService.GetList(x => x.Id == viewModel.Id).FirstOrDefault();
            if (action != null)
            {
                action.Name = viewModel.Name;
                action.ParentId = viewModel.ParentId;
                action.Code = viewModel.Code;
                action.Type = viewModel.Type;
                action.SortCode = viewModel.SortCode;
                action.UpdateTime = DateTime.Now;
                action.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
                if (viewModel.Type == ActionTypeEnum.Menu)
                {
                    action.SysActionMenu.Icon = viewModel.Icon;
                    action.SysActionMenu.Url = viewModel.Url;
                }
                else if (viewModel.Type == ActionTypeEnum.Button)
                {
                    action.SysActionButton.ButtonCode = viewModel.Code;
                    action.SysActionButton.Location = viewModel.Location;
                    action.SysActionButton.ButtonClass = viewModel.ButtonClass;
                    action.SysActionButton.ButtonIcon = viewModel.ButtonIcon;
                }
                else
                {
                    return WriteError("类型不正确");
                }
                _sysActionService.Update(action);
                return WriteSuccess();
            }
            return WriteError();
        }

        [HttpPost, CheckPermission("system.menu.delete")]
        public ActionResult Delete(int id)
        {
            try
            {
                var action = _sysActionService.GetList(x => x.Id == id).FirstOrDefault();
                if (action.Type == ActionTypeEnum.Menu)
                {
                    _sysActionMenuService.DeleteBy(x => x.Id == id);
                    _sysActionService.DeleteBy(x => x.Id == id);
                }
                else if (action.Type == ActionTypeEnum.Button)
                {
                    _sysActionButtonService.DeleteBy(x => x.Id == id);
                    _sysActionService.DeleteBy(x => x.Id == id);
                }
                return WriteSuccess("数据删除成功");
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.menu.batchDel")]
        public ActionResult BatchDel(string idsStr)
        {
            try
            {
                var idsArray = idsStr.Substring(0, idsStr.Length).Split(',');
                int[] ids = Array.ConvertAll<string, int>(idsArray, int.Parse);
                var result = _sysActionService.DeleteByIds(ids) ? WriteSuccess("数据删除成功") : WriteError("数据删除失败");
                return result;
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }
        #endregion

        #region 获取数据
        [HttpGet, CheckPermission("system.menu.list")]
        public JsonResult GetList()
        {
            var list = _sysActionService.GetList(x => true).OrderBy(x => x.SortCode).ToList();
            var result = new { code = 0, count = list.Count(), data = list };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, CheckPermission("system.role.assign")]
        public JsonResult Assign(int id)
        {
            var data = _sysActionService.GetDtree(id);
            var result = new DtreeViewModel { Data = data, Status = new DtreeStatus() };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CheckPermission("system.menu.create")]
        public JsonResult GetMenuTree()
        {
            var data = _sysActionService.GetMenuTree();
            var result = new DtreeViewModel { Data = data, Status = new DtreeStatus() };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}