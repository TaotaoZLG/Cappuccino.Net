using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Xml.Linq;
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
        private ISysTemplateService _sysTemplateService;

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

        [HttpGet, CheckPermission("system.template.edit")]
        public ActionResult Edit(long id)
        {
            SysTemplateEntity viewModel = _sysTemplateService.GetList(x => x.Id == id).FirstOrDefault();
            return View(viewModel.EntityMap());
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

                if (string.IsNullOrWhiteSpace(viewModel.TemplateFilePath))
                {
                    return WriteError("文件未上传或者上传失败");
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
            SysTemplateEntity entity = viewModel.EntityMap();
            entity.UpdateTime = DateTime.Now;
            entity.UpdateUserId = UserManager.GetCurrentUserInfo().Id;

            _sysTemplateService.Update(entity, new string[] { "TemplateName", "TemplateType", "TemplateStatus", "TemplateFilePath", "SortCode", "Remark", "UpdateTime", "UpdateUserId" });
            return WriteSuccess();
        }

        [HttpPost, CheckPermission("business.template.batchDel")]
        [LogOperate(Title = "批量删除业务模板", BusinessType = (int)OperateType.Delete)]
        public ActionResult BatchDel(string idsStr)
        {
            try
            {
                var idsArray = idsStr.Split(',').Select(x => long.Parse(x)).ToArray();
                var result = _sysTemplateService.DeleteBy(x => idsArray.Contains(x.Id)) > 0 ? WriteSuccess("数据删除成功") : WriteError("数据删除失败");
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

            var list = _sysTemplateService.GetListByPage(queries.AsExpression<SysTemplateEntity>(), pageInfo.Field, pageInfo.Order, pageInfo.Limit, pageInfo.Page, out int totalCount, includes: t => t.CreateUserId).Select(x => new
            {
                x.Id,
                x.TemplateName,
                x.TemplateContent,
                x.TemplateType,
                x.TemplateStatus,
                x.SortCode,
                x.Remark,
                x.CreateTime,
                CreateUserName = x.SysUser.UserName

            }).ToList();
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMaxSortCode()
        {
            int maxSortCode = _sysTemplateService.GetMaxSortCode();
            return WriteSuccess("查询成功", maxSortCode);
        }

        public ActionResult GetTemplate()
        {
            var template = _sysTemplateService.GetList(x => true).Select(x => new { x.Id, Name = x.TemplateName }).ToList();
            if (template == null)
            {
                return WriteError("模板不存在");
            }
            return WriteSuccess("查询成功", template);
        }


        public SelectList TemplateSelectList
        {
            get
            {
                return new SelectList(_sysTemplateService.GetList(x => true).Select(x => new { x.Id, x.TemplateName }), "Id", "Name");
            }
        }
        #endregion
    }
}