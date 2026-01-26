using System.Linq;
using System.Web.Mvc;
using Cappuccino.Common;
using Cappuccino.Entity;
using Cappuccino.IBLL.System;
using Cappuccino.Model;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysAutoJobLogController : BaseController
    {
        private readonly ISysAutoJobLogService _sysAutoJobLogService;

        public SysAutoJobLogController(ISysAutoJobLogService sysAutoJobLogService)
        {
            _sysAutoJobLogService = sysAutoJobLogService;
        }

        // GET: System/SysAutoJobLog
        public override ActionResult Index()
        {
            return View();
        }

        [CheckPermission("system.autojob.log")]
        public JsonResult GetList(SysAutoJobLogModel viewModel, PageInfo pageInfo)
        {
            var queries = new QueryCollection();
            if (!string.IsNullOrEmpty(viewModel.JobName))
            {
                queries.Add(new Query { Name = "JobName", Operator = Query.Operators.Equal, Value = viewModel.JobName });
            }
            ;

            var list = _sysAutoJobLogService.GetListByPage(
                queries.AsExpression<SysAutoJobLogEntity>(),
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
                x.StartTime,
                x.EndTime,
                x.ExecuteStatus,
                x.ExecuteResult,
                x.ExecuteDuration,
                x.Exception
            }).ToList();

            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }
    }
}