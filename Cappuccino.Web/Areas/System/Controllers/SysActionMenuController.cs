using System.Web.Mvc;
using Cappuccino.IBLL;
using Cappuccino.Web.Core;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysActionMenuController : BaseController
    {
        private readonly ISysActionMenuService _sysActionMenuService;

        public SysActionMenuController(ISysActionMenuService sysActionMenuService)
        {
            _sysActionMenuService = sysActionMenuService;
        }

        #region 视图
        [CheckPermission("system.menu.create")]
        public ActionResult CreateMenuPartial()
        {
            return PartialView("_ActionMenuPartial");
        }
        #endregion
    }
}