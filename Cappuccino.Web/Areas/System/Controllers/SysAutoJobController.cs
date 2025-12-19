using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cappuccino.Common;
using Cappuccino.Common.Enum;
using Cappuccino.Common.Extensions;
using Cappuccino.Entity;
using Cappuccino.Entity.System;
using Cappuccino.IBLL.System;
using Cappuccino.Model;
using Cappuccino.Web.Attributes;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysAutoJobController : BaseController
    {
        private readonly ISysAutoJobService _sysAutoJobService;

        public SysAutoJobController(ISysAutoJobService sysAutoJobService)
        {
            _sysAutoJobService = sysAutoJobService;
            AddDisposableObject(_sysAutoJobService);
        }

        #region 视图
        [CheckPermission("system.autojob.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }

        [HttpGet, CheckPermission("system.autojob.create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet, CheckPermission("system.autojob.edit")]
        public ActionResult Edit(int id)
        {
            var viewModel = _sysAutoJobService.GetList(x => x.Id == id).FirstOrDefault();
            return View(viewModel.EntityMap());
        }

        [HttpGet, CheckPermission("system.autojob.log")]
        public ActionResult Log(int jobId)
        {
            ViewBag.JobId = jobId;
            return View();
        }
        #endregion

        #region 提交数据
        [HttpPost, CheckPermission("system.autojob.create")]
        [LogOperate(Title = "新增任务计划", BusinessType = (int)OperateType.Add)]
        public ActionResult Create(SysAutoJobModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return WriteError("实体验证失败");
                }
                SysAutoJobEntity entity = viewModel.EntityMap();
                entity.CreateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.CreateTime = DateTime.Now;
                entity.UpdateTime = DateTime.Now;

                _sysAutoJobService.Insert(entity);

                return WriteSuccess();
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.autojob.edit")]
        [LogOperate(Title = "编辑任务计划", BusinessType = (int)OperateType.Update)]
        public ActionResult Edit(SysAutoJobModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return WriteError("实体验证失败");
                }

                viewModel.Id = viewModel.Id;
                viewModel.UpdateTime = DateTime.Now;
                viewModel.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
                SysAutoJobEntity entity = viewModel.EntityMap();

                _sysAutoJobService.Update(entity, new string[] { "JobName", "JobGroup", "Description", "JobClassName", "UpdateTime", "CronExpression", "StartTime", "EndTime", "UpdateTime", "UpdateUserId" });
                return WriteSuccess();
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.autojob.delete")]
        [LogOperate(Title = "删除任务计划", BusinessType = (int)OperateType.Delete)]
        public ActionResult Delete(int id)
        {
            try
            {
                _sysAutoJobService.DeleteBy(x => x.Id == id);
                return WriteSuccess("删除成功");
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }
        #endregion

        #region 任务管控
        //[HttpPost, CheckPermission("system.autojob.start")]
        //[LogOperate(Title = "启动任务计划", BusinessType = (int)OperateType.Other)]
        //public ActionResult Start(int id)
        //{
        //    var result = _sysAutoJobService.StartJob(id).Result;
        //    return result ? WriteSuccess("启动成功") : WriteError("启动失败");
        //}

        //[HttpPost, CheckPermission("system.autojob.stop")]
        //[LogOperate(Title = "停止任务计划", BusinessType = (int)OperateType.Other)]
        //public ActionResult Stop(int id)
        //{
        //    var result = _sysAutoJobService.StopJob(id).Result;
        //    return result ? WriteSuccess("停止成功") : WriteError("停止失败");
        //}

        //[HttpPost, CheckPermission("system.autojob.execute")]
        //[LogOperate(Title = "立即执行任务", BusinessType = (int)OperateType.Other)]
        //public ActionResult ExecuteImmediately(int id)
        //{
        //    var result = _sysAutoJobService.ExecuteJob(id).Result;
        //    return result ? WriteSuccess("执行命令已发送") : WriteError("执行失败");
        //}
        #endregion

        #region 数据获取
        [CheckPermission("system.autojob.list")]
        public JsonResult GetList(SysAutoJobModel viewModel, PageInfo pageInfo)
        {
            var queries = new QueryCollection();
            if (!string.IsNullOrEmpty(viewModel.JobName))
            {
                queries.Add(new Query { Name = "JobName", Operator = Query.Operators.Contains, Value = viewModel.JobName });
            }
            if (!string.IsNullOrEmpty(viewModel.JobGroup))
            {
                queries.Add(new Query { Name = "JobGroup", Operator = Query.Operators.Contains, Value = viewModel.JobGroup });
            }
            if (viewModel.JobStatus != null)
            {
                queries.Add(new Query { Name = "JobStatus", Operator = Query.Operators.Equal, Value = viewModel.JobStatus });
            }

            var list = _sysAutoJobService.GetListByPage(
                queries.AsExpression<SysAutoJobEntity>(),
                pageInfo.Field,
                pageInfo.Order,
                pageInfo.Limit,
                pageInfo.Page,
                out int totalCount
            ).Select(x => new
            {
                x.Id,
                x.JobName,
                x.JobGroup,
                x.Description,
                x.JobClassName,
                x.CronExpression,
                x.JobStatus,
                x.StartTime,
                x.EndTime,
                x.LastExecuteTime,
                x.NextExecuteTime
            }).ToList();

            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}