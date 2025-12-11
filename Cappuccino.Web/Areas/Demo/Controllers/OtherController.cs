using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cappuccino.Web.Areas.Demo.Controllers
{
    public class OtherController : Controller
    {
        // GET: Demo/Other
        public ActionResult Watermark()
        {
            return View();
        }

        public ActionResult Encrypt()
        {
            return View();
        }

        public ActionResult Fullscreen()
        {
            return View();
        }
    }
}