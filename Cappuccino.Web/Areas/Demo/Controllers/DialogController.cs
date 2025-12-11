using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cappuccino.Web.Areas.Demo.Controllers
{
    public class DialogController : Controller
    {
        // GET: Demo/Dialog
        public ActionResult Toast()
        {
            return View();
        }

        public ActionResult Popup()
        {
            return View();
        }

        public ActionResult Notice()
        {
            return View();
        }

        public ActionResult Drawer() 
        { 
            return View(); 
        }
    }
}