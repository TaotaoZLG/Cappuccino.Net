using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cappuccino.Web.Core;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysConfigController : BaseController
    {
       
        #region 视图
        [CheckPermission("system.config.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }

        [HttpGet, CheckPermission("system.config.create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet, CheckPermission("system.config.edit")]
        public ActionResult Edit(int id)
        {
            return View();
        }
        #endregion
    }
}