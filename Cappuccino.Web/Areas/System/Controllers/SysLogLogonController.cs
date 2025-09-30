using System;
using System.Linq;
using System.Web.Mvc;
using Cappuccino.Common;
using Cappuccino.Common.Helper;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.Model;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysLogLogonController : BaseController
    {
        private readonly ISysLogLogonService _sysLogLogonService;

        public SysLogLogonController(ISysLogLogonService sysLogLogonService)
        {
            _sysLogLogonService = sysLogLogonService;
            this.AddDisposableObject(_sysLogLogonService);
        }

        #region 视图
        [CheckPermission("system.loglogon.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }
        #endregion

        #region 获取数据
        [CheckPermission("system.loglogon.list")]
        public JsonResult GetList(SysLogLogonModel viewModel, PageInfo pageInfo)
        {
            QueryCollection queries = new QueryCollection();
            if (!string.IsNullOrEmpty(viewModel.RealName))
            {
                queries.Add(new Query { Name = "RealName", Operator = Query.Operators.Contains, Value = viewModel.RealName });

            }
            if (!string.IsNullOrEmpty(viewModel.Account))
            {
                queries.Add(new Query { Name = "Account", Operator = Query.Operators.Contains, Value = viewModel.Account });

            }
            if (!string.IsNullOrEmpty(viewModel.StartEndDate))
            {
                queries.Add(new Query { Name = "CreateTime", Operator = Query.Operators.GreaterThanOrEqual, Value = StartEndDateHelper.GteStartDate(viewModel.StartEndDate) });
                queries.Add(new Query { Name = "CreateTime", Operator = Query.Operators.LessThanOrEqual, Value = StartEndDateHelper.GteEndDate(viewModel.StartEndDate) });
            }
            var list = _sysLogLogonService.GetListByPage(queries.AsExpression<SysLogLogonEntity>(), pageInfo.Field, pageInfo.Order, pageInfo.Limit, pageInfo.Page, out int totalCount).Select(x => new
            {
                x.Id,
                x.LogType,
                x.Account,
                x.RealName,
                x.Description,
                x.IPAddress,
                x.IPAddressName,
                x.CreateTime
            }).ToList();
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}