using System.Web.Mvc;
using Cappuccino.IBLL;
using Cappuccino.Web.Core;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysActionMenuController : BaseController
    {
        public SysActionMenuController(ISysActionMenuService sysActionMenuService)
        {
            base.SysActionMenuService = sysActionMenuService;
            this.AddDisposableObject(SysActionMenuService);
        }

        [CheckPermission("system.menu.create")]
        public ActionResult CreateMenuPartial()
        {
            return PartialView("_ActionMenuPartial");
        }
    }
}