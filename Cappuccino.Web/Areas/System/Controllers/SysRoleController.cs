using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cappuccino.Common;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.Model;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysRoleController : BaseController
    {
        private readonly ISysRoleService _sysRoleService;

        public SysRoleController(ISysRoleService sysRoleService)
        {
            _sysRoleService = sysRoleService;
        }

        #region 视图
        [CheckPermission("system.role.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }

        [HttpGet, CheckPermission("system.role.create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet, CheckPermission("system.role.edit")]
        public ActionResult Edit(int id)
        {
            var viewModel = _sysRoleService.GetList(x => x.Id == id).FirstOrDefault();
            return View(viewModel.EntityMap());
        }

        [CheckPermission("system.role.assign")]
        public ActionResult Assign(int id)
        {
            ViewBag.RoleId = id;
            return View();
        }
        #endregion

        #region 提交数据
        [HttpPost, CheckPermission("system.role.create")]
        public ActionResult Create(SysRoleViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return WriteError("实体验证失败");
                }
                SysRoleEntity entity = viewModel.EntityMap();
                entity.CreateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.CreateTime = DateTime.Now;
                entity.UpdateTime = DateTime.Now;
                _sysRoleService.Add(entity);
                return WriteSuccess();
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.role.edit")]
        public ActionResult Edit(int id, SysRoleViewModel viewModel)
        {
            if (ModelState.IsValid == false)
            {
                return WriteError("实体验证失败");
            }
            viewModel.Id = id;
            viewModel.UpdateTime = DateTime.Now;
            viewModel.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
            SysRoleEntity entity = viewModel.EntityMap();
            _sysRoleService.Update(entity, new string[] { "Name", "Code", "EnabledMark", "Remark", "UpdateTime", "UpdateUserId" });
            return WriteSuccess();
        }

        [HttpPost, CheckPermission("system.role.delete")]
        public ActionResult Delete(int id)
        {
            try
            {
                _sysRoleService.DeleteBy(x => x.Id == id);
                return WriteSuccess("数据删除成功");
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.role.batchDel")]
        public ActionResult BatchDel(string idsStr)
        {
            try
            {
                var idsArray = idsStr.Substring(0, idsStr.Length).Split(',');
                int[] ids = Array.ConvertAll<string, int>(idsArray, int.Parse);
                var result = _sysRoleService.DeleteBy(x => ids.Contains(x.Id)) > 0 ? WriteSuccess("数据删除成功") : WriteError("数据删除失败");
                return result;
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.role.assign")]
        public ActionResult Assign(int id, List<DtreeResponse> dtrees)
        {
            _sysRoleService.Add(id, dtrees);
            return WriteSuccess("保存成功");
        }

        [HttpPost, CheckPermission("system.role.edit")]
        public ActionResult UpdateEnabledMark(int id, int enabledMark)
        {
            SysRoleEntity entity = new SysRoleEntity
            {
                Id = id,
                EnabledMark = enabledMark,
                UpdateTime = DateTime.Now,
                UpdateUserId = UserManager.GetCurrentUserInfo().Id
            };
            _sysRoleService.Update(entity, new string[] { "EnabledMark", "UpdateTime", "UpdateUserId" });
            return WriteSuccess("更新成功");
        }
        #endregion

        #region 获取数据
        [CheckPermission("system.role.list")]
        public JsonResult GetList(SysRoleViewModel viewModel, PageInfo pageInfo)
        {
            QueryCollection queries = new QueryCollection();
            if (!string.IsNullOrEmpty(viewModel.Name))
            {
                queries.Add(new Query { Name = "Name", Operator = Query.Operators.Contains, Value = viewModel.Name });

            }
            else if (!string.IsNullOrEmpty(viewModel.Code))
            {
                queries.Add(new Query { Name = "Code", Operator = Query.Operators.Contains, Value = viewModel.Code });
            }
            var list = _sysRoleService.GetListByPage(queries.AsExpression<SysRoleEntity>(), x => true, pageInfo.Limit, pageInfo.Page, out int totalCount, true).Select(x => new
            {
                x.Id,
                x.Name,
                x.Code,
                x.EnabledMark,
                x.Remark
            }).ToList();
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}