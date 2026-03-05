using System.Web.Mvc;

namespace Cappuccino.Web.Areas.DemoManage.Controllers
{
    public class DialogController : Controller
    {
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