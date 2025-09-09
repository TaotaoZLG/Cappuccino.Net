using System.Web.Mvc;
using Cappuccino.IBLL;
using Cappuccino.Web.Core;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysActionButtonController : BaseController
    {
        public SysActionButtonController(ISysActionButtonService sysActionButtonService)
        {
            base.SysActionButtonService = sysActionButtonService;
            this.AddDisposableObject(SysActionButtonService);
        }

        [CheckPermission("system.menu.create")]
        public ActionResult CreateButtonPartial()
        {
            return PartialView("_ActionButtonPartial");
        }
    }
}