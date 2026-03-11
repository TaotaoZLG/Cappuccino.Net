using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cappuccino.BLL;
using Cappuccino.Common;
using Cappuccino.Common.Enum;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.Model;
using Cappuccino.Web.Attributes;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;

namespace Cappuccino.Web.Areas.BusinessManage.Controllers
{
    public class SysTemplateController : BaseController
    {
        private readonly ISysTemplateService _sysTemplateService;

        public SysTemplateController(ISysTemplateService sysTemplateService)
        {
            _sysTemplateService = sysTemplateService;
            this.AddDisposableObject(_sysTemplateService);
        }
        #region 视图
        [CheckPermission("business.template.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }

        [HttpGet, CheckPermission("business.template.create")]
        public ActionResult Create()
        {
            return View();
        }
        #endregion

        #region 提交数据
        [HttpPost, CheckPermission("business.template.create")]
        [LogOperate(Title = "新增业务模板", BusinessType = (int)OperateType.Add)]
        public ActionResult Create(SysTemplateModel viewModel)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return WriteError("实体验证失败");
                }

                SysTemplateEntity entity = viewModel.EntityMap();
                entity.CreateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.Create();

                _sysTemplateService.Insert(entity);

                return WriteSuccess();
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("business.template.edit")]
        [LogOperate(Title = "编辑业务模板", BusinessType = (int)OperateType.Update)]
        public ActionResult Edit(SysTemplateModel viewModel)
        {
            if (ModelState.IsValid == false)
            {
                return WriteError("实体验证失败");
            }
            viewModel.Id = viewModel.Id;
            viewModel.UpdateTime = DateTime.Now;
            viewModel.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
            SysTemplateEntity entity = viewModel.EntityMap();
            _sysTemplateService.Update(entity, new string[] { "Name", "Code", "SortCode", "UpdateTime", "UpdateUserId" });
            return WriteSuccess();
        }

        [HttpPost, CheckPermission("business.template.batchDel")]
        [LogOperate(Title = "批量删除用户", BusinessType = (int)OperateType.Delete)]
        public ActionResult BatchDel(string idsStr)
        {
            try
            {
                var idsArray = idsStr.Substring(0, idsStr.Length).Split(',');
                long[] ids = Array.ConvertAll<string, long>(idsArray, long.Parse);
                var result = _sysTemplateService.DeleteBy(x => ids.Contains(x.Id)) > 0 ? WriteSuccess("数据删除成功") : WriteError("数据删除失败");
                return result;
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }
        #endregion

        #region 获取数据
        [CheckPermission("business.template.list")]
        public JsonResult GetList(SysTemplateModel viewModel, PageInfo pageInfo)
        {
            QueryCollection queries = new QueryCollection();
            if (!string.IsNullOrEmpty(viewModel.TemplateName))
            {
                queries.Add(new Query { Name = "TemplateName", Operator = Query.Operators.Contains, Value = viewModel.TemplateName });
            }
           
            var list = _sysTemplateService.GetListByPage(queries.AsExpression<SysTemplateEntity>(), pageInfo.Field, pageInfo.Order, pageInfo.Limit, pageInfo.Page, out int totalCount,includes: t => t.CreateUserId).Select(x => new
            {
                x.Id,
                x.TemplateName,
                x.TemplateContent,
                x.TemplateType,
                x.TemplateStatus,
                x.SortCode,
                x.CreateTime,
                CreateUserId = x.SysUser.Id

            }).ToList();
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMaxSortCode()
        {
            int maxSortCode = _sysTemplateService.GetMaxSortCode();
            var result = new { Status = 0, Message = "查询成功", Data = maxSortCode };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}