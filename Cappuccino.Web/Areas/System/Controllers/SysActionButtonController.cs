using System.Web.Mvc;
using Cappuccino.IBLL;
using Cappuccino.Web.Core;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysActionButtonController : BaseController
    {
        private readonly ISysActionButtonService _sysActionButtonService;

        public SysActionButtonController(ISysActionButtonService sysActionButtonService)
        {
            _sysActionButtonService = sysActionButtonService;
        }

        #region 视图
        [CheckPermission("system.menu.create")]
        public ActionResult CreateButtonPartial()
        {
            return PartialView("_ActionButtonPartial");
        }
        #endregion
    }
}