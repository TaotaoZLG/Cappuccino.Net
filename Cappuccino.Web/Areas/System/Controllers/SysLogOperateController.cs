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
    public class SysLogOperateController : BaseController
    {
        private readonly ISysLogOperateService _sysLogOperateService;

        public SysLogOperateController(ISysLogOperateService sysLogOperateService)
        {
            _sysLogOperateService = sysLogOperateService;
            this.AddDisposableObject(sysLogOperateService);
        }

        [CheckPermission("system.log.operate")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }

        [CheckPermission("system.log.operate")]
        public JsonResult List(SysLogLogonViewModel viewModel, PageInfo pageInfo)
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
            var list = _sysLogOperateService.GetListByPage(queries.AsExpression<SysLogOperateEntity>(), x => true, pageInfo.Limit, pageInfo.Page, out int totalCount, true).Select(x => new
            {
                x.Id,
                x.Description,
                x.IPAddress,
                x.IPAddressName,
                x.CreateTime
            }).ToList();
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }
    }
}