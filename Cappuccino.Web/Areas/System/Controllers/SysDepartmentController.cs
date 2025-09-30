using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cappuccino.Common;
using Cappuccino.Common.Enum;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.Model;
using Cappuccino.Web.Attributes;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysDepartmentController : BaseController
    {
        private readonly ISysDepartmentService _sysDepartmentService;

        public SysDepartmentController(ISysDepartmentService sysDepartmentService)
        {
            _sysDepartmentService = sysDepartmentService;
        }

        #region 视图
        [CheckPermission("system.department.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }

        [HttpGet, CheckPermission("system.department.create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet, CheckPermission("system.department.edit")]
        public ActionResult Edit(int id)
        {
            SysDepartmentEntity departmentEntity = _sysDepartmentService.GetList(x => x.Id == id).FirstOrDefault();
            return View(departmentEntity);
        }
        #endregion

        #region 提交数据
        [HttpPost, CheckPermission("system.department.create")]
        [LogOperate(Title = "新增部门", BusinessType = (int)OperateType.Add)]
        public ActionResult Create(SysDepartmentModel model)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return WriteError("实体验证失败");
                }

                SysDepartmentEntity entity = new SysDepartmentEntity();
                entity.CreateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.CreateTime = DateTime.Now;
                entity.UpdateTime = DateTime.Now;
                _sysDepartmentService.Add(entity);

                return WriteSuccess();
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.department.edit")]
        [LogOperate(Title = "编辑部门", BusinessType = (int)OperateType.Update)]
        public ActionResult Edit(SysDepartmentModel model)
        {
            if (ModelState.IsValid == false)
            {
                return WriteError("实体验证失败");
            }

            SysDepartmentEntity departmentEntity = new SysDepartmentEntity();
            departmentEntity.Id = model.Id;
            departmentEntity.UpdateTime = DateTime.Now;
            departmentEntity.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
            _sysDepartmentService.Update(departmentEntity, new string[] { "Name", "Code", "SortCode", "UpdateTime", "UpdateUserId" });
            return WriteSuccess();
        }

        [HttpPost, CheckPermission("system.department.delete")]
        [LogOperate(Title = "删除部门", BusinessType = (int)OperateType.Delete)]
        public ActionResult Delete(int id)
        {
            try
            {
                _sysDepartmentService.DeleteBy(x => x.Id == id);
                return WriteSuccess("数据删除成功");
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }
        #endregion

        #region 获取数据
        [CheckPermission("system.department.list")]
        public JsonResult GetList(SysDepartmentModel viewModel)
        {
            QueryCollection queries = new QueryCollection();
            if (!string.IsNullOrEmpty(viewModel.Name))
            {
                queries.Add(new Query { Name = "Name", Operator = Query.Operators.Contains, Value = viewModel.Name });
            }
            List<SysDepartmentEntity> list = _sysDepartmentService.GetList(queries.AsExpression<SysDepartmentEntity>()).OrderBy(x => x.SortCode).ToList();

            var result = new { code = 0, count = list.Count(), data = list };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}