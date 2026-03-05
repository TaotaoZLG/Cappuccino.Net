using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cappuccino.DataAccess;
using Cappuccino.Model;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;

namespace Cappuccino.Web.Areas.BusinessManage.Controllers
{
    public class SysCaseInfoController : BaseController
    {
        [CheckPermission("business.case.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }

        #region 获取数据
        [CheckPermission("system.case.list")]
        public JsonResult GetList(SysCaseInfoModel viewModel, PageInfo pageInfo)
        {
            var coreSql = $@"
                SELECT * FROM dbo.SysCaseInfo c
                WHERE 1=1
                {(!string.IsNullOrEmpty(viewModel.CustIDNumber) ? $"AND c.CustIDNumber LIKE '%{viewModel.CustIDNumber}%'" : "")}
                {(!string.IsNullOrEmpty(viewModel.CustName) ? $"AND c.CustName LIKE '%{viewModel.CustName}%'" : "")}
            ";

            var list = DapperHelper.QueryPage<dynamic>(coreSql, pageInfo.Field, pageInfo.Order, pageInfo.Limit, pageInfo.Page, out int totalCount);
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}