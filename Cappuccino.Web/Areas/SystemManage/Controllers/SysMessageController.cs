using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cappuccino.Web.Core;

namespace Cappuccino.Web.Areas.SystemManage.Controllers
{
    public class SysMessageController : BaseController
    {
        [CheckPermission("system.message.list")]
        public override ActionResult Index()
        {
            return View();
        }

        public ActionResult Detail(long id)
        {
            return View();
        }
    }
}