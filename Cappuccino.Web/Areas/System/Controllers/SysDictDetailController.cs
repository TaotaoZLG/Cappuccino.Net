using System;
using System.Linq;
using System.Web.Mvc;
using Cappuccino.BLL;
using Cappuccino.Common;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.Model;
using Cappuccino.Web.Attributes;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysDictDetailController : BaseController
    {
        private readonly ISysDictDetailService _sysDictDetailService;
        private readonly ISysActionButtonService _sysActionButtonService;

        public SysDictDetailController(ISysDictDetailService sysDictDetailService, ISysActionButtonService sysActionButtonService)
        {
            _sysDictDetailService = sysDictDetailService;
            _sysActionButtonService = sysActionButtonService;
            this.AddDisposableObject(_sysDictDetailService);
            this.AddDisposableObject(_sysActionButtonService);
        }

        #region 视图        
        [HttpGet, CheckPermission("system.dict.create")]
        public ActionResult Create(int TypeId)
        {
            ViewBag.TypeId = TypeId;
            return View();
        }

        [HttpGet, CheckPermission("system.dict.edit")]
        public ActionResult Edit(int id)
        {
            var viewModel = _sysDictDetailService.GetList(x => x.Id == id).FirstOrDefault();
            return View(viewModel.EntityMap());
        }
        #endregion

        #region 提交数据
        [HttpPost, CheckPermission("system.dict.create")]
        [LogOperate(Title = "新增字典详情", BusinessType = "ADD")]
        public ActionResult Create(SysDictDetailViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return WriteError("实体验证失败");
                }
                SysDictDetailEntity entity = viewModel.EntityMap();
                entity.CreateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.CreateTime = DateTime.Now;
                entity.UpdateTime = DateTime.Now;
                _sysDictDetailService.Add(entity);
                return WriteSuccess();
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.dict.edit")]
        [LogOperate(Title = "编辑字典详情", BusinessType = "EDIT")]
        public ActionResult Edit(SysDictDetailViewModel viewModel)
        {
            if (ModelState.IsValid == false)
            {
                return WriteError("实体验证失败");
            }
            viewModel.Id = viewModel.Id;
            viewModel.UpdateTime = DateTime.Now;
            viewModel.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
            SysDictDetailEntity entity = viewModel.EntityMap();
            _sysDictDetailService.Update(entity, new string[] { "Name", "Code", "SortCode", "UpdateTime", "UpdateUserId" });
            return WriteSuccess();
        }

        [HttpPost, CheckPermission("system.dict.delete")]
        [LogOperate(Title = "删除字典详情", BusinessType = "DELETE")]
        public ActionResult Delete(int id)
        {
            try
            {
                _sysDictDetailService.DeleteBy(x => x.Id == id);
                return WriteSuccess("数据删除成功");
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.dict.batchDel")]
        [LogOperate(Title = "批量删除字典详情", BusinessType = "DELETE")]
        public ActionResult BatchDel(string idsStr)
        {
            try
            {
                var idsArray = idsStr.Substring(0, idsStr.Length).Split(',');
                int[] ids = Array.ConvertAll<string, int>(idsArray, int.Parse);
                var result = _sysDictDetailService.DeleteBy(x => ids.Contains(x.Id)) > 0 ? WriteSuccess("数据删除成功") : WriteError("数据删除失败");
                return result;
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }
        #endregion

        #region 获取数据
        [CheckPermission("system.dict.list")]
        public JsonResult GetList(SysDictDetailViewModel viewModel, PageInfo pageInfo)
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
            else if (viewModel.TypeId != 0)
            {
                queries.Add(new Query { Name = "TypeId", Operator = Query.Operators.Equal, Value = viewModel.TypeId });
            }
            var list = _sysDictDetailService.GetListByPage(queries.AsExpression<SysDictDetailEntity>(), pageInfo.Field, pageInfo.Order, pageInfo.Limit, pageInfo.Page, out int totalCount).Select(x => new
            {
                x.Id,
                x.Name,
                x.Code,
                x.SortCode
            }).ToList();
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}