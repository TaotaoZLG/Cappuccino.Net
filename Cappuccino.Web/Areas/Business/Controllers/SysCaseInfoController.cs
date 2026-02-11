using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cappuccino.Web.Core;

namespace Cappuccino.Web.Areas.Business.Controllers
{
    public class SysCaseInfoController : Controller
    {
        // GET: Business/SysCaseInfo
        [CheckPermission("business.casebrowse.list")]
        public ActionResult Index()
        {
            return View();
        }
    }
}