using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cappuccino.Web.Core;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysAutoJobLogController : BaseController
    {
        // GET: System/SysAutoJobLog
        public override ActionResult Index()
        {
            return View();
        }
    }
}