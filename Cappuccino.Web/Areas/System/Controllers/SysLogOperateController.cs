using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cappuccino.Common;
using Cappuccino.Common.Enum;
using Cappuccino.Common.Helper;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.Model;
using Cappuccino.Web.Attributes;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;
using MiniExcelLibs;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysLogOperateController : BaseController
    {
        private readonly ISysLogOperateService _sysLogOperateService;

        public SysLogOperateController(ISysLogOperateService sysLogOperateService)
        {
            _sysLogOperateService = sysLogOperateService;
            this.AddDisposableObject(_sysLogOperateService);
        }

        #region 视图
        [CheckPermission("system.logoperate.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }
        #endregion

        #region 获取数据
        [CheckPermission("system.logoperate.list")]
        public JsonResult GetList(SysLogOperateModel viewModel, PageInfo pageInfo)
        {
            var queries = BuildUserQueries(viewModel);
            var list = _sysLogOperateService.GetListByPage(queries.AsExpression<SysLogOperateEntity>(), pageInfo.Field, pageInfo.Order, pageInfo.Limit, pageInfo.Page, out int totalCount).ToList();
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }

        [CheckPermission("system.logoperate.export")]
        [LogOperate(Title = "导出操作日志", BusinessType = (int)OperateType.Export)]
        public ActionResult ExportLogOperate(SysLogOperateModel viewModel, string checkedIds = null)
        {
            try
            {
                var queries = BuildUserQueries(viewModel);

                if (!string.IsNullOrEmpty(checkedIds))
                {
                    // 导出勾选的用户（拆分ID列表）
                    var ids = checkedIds.Split(',');
                    queries.Add(new Query { Name = "Id", Operator = Query.Operators.In, Value = ids });
                }

                var logOperateList = _sysLogOperateService.GetList(queries.AsExpression<SysLogOperateEntity>());

                var file = new ExcelHelper<SysLogOperateEntity>().ExportToExcel("操作日志.xlsx","操作日志", logOperateList.ToList(), null);

                return WriteSuccess("导出成功", file);
                // 转换为导出模型（避免直接暴露实体，只包含需要的字段）
                //var exportData = logOperateList.Select(user => new
                //{
                //    用户ID = user.Id,
                //    用户名 = user.UserName,
                //    昵称 = user.NickName,
                //    部门 = user.Department?.Name ?? "无部门",
                //    手机号 = user.MobilePhone,
                //    邮箱 = user.Email,
                //    状态 = user.EnabledMark == (int)EnabledMarkEnum.Valid ? "启用" : "禁用",
                //    角色 = string.Join("，", user.SysRoles.Select(r => r.Name)),
                //    创建时间 = user.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
                //}).ToList();
            }
            catch (Exception ex)
            {
                // 记录日志并返回错误信息
                return WriteError($"导出失败：{ex.Message}");
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 复用查询条件构建逻辑（与GetList统一，避免重复代码）
        /// </summary>
        private QueryCollection BuildUserQueries(SysLogOperateModel viewModel)
        {
            QueryCollection queries = new QueryCollection();
            if (!string.IsNullOrEmpty(viewModel.RequestUrl))
            {
                queries.Add(new Query { Name = "RequestUrl", Operator = Query.Operators.Contains, Value = viewModel.RequestUrl });
            }
            if (!string.IsNullOrEmpty(viewModel.OperateName))
            {
                queries.Add(new Query { Name = "OperateName", Operator = Query.Operators.Equal, Value = viewModel.OperateName });
            }
            if (!string.IsNullOrEmpty(viewModel.IPAddress))
            {
                queries.Add(new Query { Name = "IPAddress", Operator = Query.Operators.Equal, Value = viewModel.IPAddress });
            }
            if (!string.IsNullOrEmpty(viewModel.StartEndDate))
            {
                queries.Add(new Query { Name = "CreateTime", Operator = Query.Operators.GreaterThanOrEqual, Value = StartEndDateHelper.GteStartDate(viewModel.StartEndDate) });
                queries.Add(new Query { Name = "CreateTime", Operator = Query.Operators.LessThanOrEqual, Value = StartEndDateHelper.GteEndDate(viewModel.StartEndDate) });
            }
            return queries;
        }
        #endregion
    }
}