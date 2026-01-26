using System.Web.Mvc;

namespace Cappuccino.Web.Areas.Demo.Controllers
{
    public class IconController : Controller
    {
        // GET: Demo/Icon
        public ActionResult Icon()
        {
            return View();
        }

        public ActionResult IconPicker()
        {
            return View();
        }
    }
}