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

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysNoticeController : BaseController
    {
        private readonly ISysNoticeService _noticeService;

        public SysNoticeController(ISysNoticeService noticeService)
        {
            _noticeService = noticeService;
            AddDisposableObject(_noticeService);
        }

        // GET: System/Notice
        #region 视图
        [CheckPermission("system.notice.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }

        [HttpGet, CheckPermission("system.notice.create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet, CheckPermission("system.notice.edit")]
        public ActionResult Edit(int id)
        {
            var viewModel = _noticeService.GetList(x => x.Id == id).FirstOrDefault();
            return View(viewModel.EntityMap());
        }
        #endregion

        #region 提交数据
        [HttpPost, CheckPermission("system.notice.create")]
        [LogOperate(Title = "新增通知公告", BusinessType = (int)OperateType.Add)]
        public ActionResult Create(SysNoticeModel viewModel)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return WriteError("实体验证失败");
                }
                SysNoticeEntity entity = viewModel.EntityMap();
                entity.CreateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.CreateTime = DateTime.Now;
                entity.UpdateTime = DateTime.Now;
                _noticeService.Insert(entity);
                return WriteSuccess();
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.notice.edit")]
        [LogOperate(Title = "编辑通知公告", BusinessType = (int)OperateType.Update)]
        public ActionResult Edit(int id, SysNoticeModel viewModel)
        {
            if (ModelState.IsValid == false)
            {
                return WriteError("实体验证失败");
            }
            viewModel.Id = id;
            viewModel.UpdateTime = DateTime.Now;
            viewModel.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
            SysNoticeEntity entity = viewModel.EntityMap();
            _noticeService.Update(entity, new string[] { "Name", "Code", "EnabledMark", "Remark", "UpdateTime", "UpdateUserId" });
            return WriteSuccess();
        }

        [HttpPost, CheckPermission("system.notice.delete")]
        [LogOperate(Title = "删除通知公告", BusinessType = (int)OperateType.Delete)]
        public ActionResult Delete(int id)
        {
            try
            {
                _noticeService.DeleteBy(x => x.Id == id);
                return WriteSuccess("数据删除成功");
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }
        #endregion

        #region 获取数据
        [CheckPermission("system.notice.list")]
        public JsonResult GetList(SysNoticeModel viewModel, PageInfo pageInfo)
        {
            QueryCollection queries = new QueryCollection();
            if (!string.IsNullOrEmpty(viewModel.NoticeTitle))
            {
                queries.Add(new Query { Name = "NoticeTitle", Operator = Query.Operators.Contains, Value = viewModel.NoticeTitle });
            }
            
            var list = _noticeService.GetListByPage(queries.AsExpression<SysNoticeEntity>(), pageInfo.Field, pageInfo.Order, pageInfo.Limit, pageInfo.Page, out int totalCount).Select(x => new
            {
                x.Id,
                x.NoticeTitle,
                x.NoticeContents,
                x.SortCode,
                x.NoticeSender,
                x.NoticeAccept,
                x.Remark
            }).ToList();
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}