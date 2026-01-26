using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cappuccino.Common.Enum;
using Cappuccino.IBLL;
using Cappuccino.Model;
using Cappuccino.Web.Attributes;
using Cappuccino.Web.Core;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysUserActionController : BaseController
    {
        private readonly ISysUserActionService _sysUserActionService;

        public SysUserActionController(ISysUserActionService sysUserActionService)
        {
            _sysUserActionService = sysUserActionService;
        }

        #region 视图

        #endregion

        #region 提交数据
        [HttpPost, CheckPermission("system.user.assign")]
        [LogOperate(Title = "用户授权", BusinessType = (int)OperateType.Authorize)]
        public ActionResult Save(int userId, List<UserActionModel> list)
        {
            var result = _sysUserActionService.SaveUserAction(userId, list) ? WriteSuccess("保存成功") : WriteError("保存失败");
            return result;
        }
        #endregion

        #region 获取数据
        [HttpGet, CheckPermission("system.user.assign")]
        public ActionResult GetList(int id)
        {
            var list = _sysUserActionService.GetUserActionList(id);
            var result = new { code = 0, count = list.Count(), data = list };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}