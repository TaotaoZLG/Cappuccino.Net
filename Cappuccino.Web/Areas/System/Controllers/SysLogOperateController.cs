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
            this.AddDisposableObject(_sysLogOperateService);
        }

        #region 视图
        [CheckPermission("system.log.operate")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }
        #endregion

        #region 获取数据
        [CheckPermission("system.log.operate")]
        public JsonResult GetList(SysLogOperateViewModel viewModel, PageInfo pageInfo)
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
            var list = _sysLogOperateService.GetListByPage(queries.AsExpression<SysLogOperateEntity>(), pageInfo.Field, pageInfo.Order, pageInfo.Limit, pageInfo.Page, out int totalCount).ToList();
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}