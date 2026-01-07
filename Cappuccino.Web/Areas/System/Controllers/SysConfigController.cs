using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cappuccino.BLL;
using Cappuccino.BLL.System;
using Cappuccino.Common;
using Cappuccino.Common.Caching;
using Cappuccino.Common.Enum;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.Model;
using Cappuccino.Web.Attributes;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysConfigController : BaseController
    {
        private readonly ISysConfigService _configService;

        public SysConfigController(ISysConfigService configService) 
        {
            _configService = configService;
            this.AddDisposableObject(_configService);
        }
       
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
            SysConfigEntity viewModel = _configService.GetList(x => x.Id == id).FirstOrDefault();
            return View(viewModel.EntityMap());
        }
        #endregion

        #region 提交数据
        [HttpPost, CheckPermission("system.config.create")]
        [LogOperate(Title = "新增系统参数", BusinessType = (int)OperateType.Add)]
        public ActionResult Create(SysConfigModel viewModel)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return WriteError("实体验证失败");
                }
                SysConfigEntity entity = viewModel.EntityMap();
                entity.CreateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.CreateTime = DateTime.Now;
                entity.UpdateTime = DateTime.Now;
                _configService.Insert(entity);
                return WriteSuccess();
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.config.edit")]
        [LogOperate(Title = "编辑系统参数", BusinessType = (int)OperateType.Update)]
        public ActionResult Edit(int id, SysConfigModel viewModel)
        {
            if (ModelState.IsValid == false)
            {
                return WriteError("实体验证失败");
            }
            viewModel.Id = id;
            viewModel.UpdateTime = DateTime.Now;
            viewModel.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
            SysConfigEntity entity = viewModel.EntityMap();
            _configService.Update(entity, new string[] { "ConfigName", "ConfigKeys", "ConfigValue", "ConfigType", "Remark", "UpdateTime", "UpdateUserId" });

            // 如果修改的是IP黑名单配置，刷新缓存
            if (entity.ConfigKeys == "sys_ipBlackList")
            {
                CacheManager.Remove(KeyManager.IpBlackCacheKey);
            }

            return WriteSuccess();
        }

        [HttpPost, CheckPermission("system.config.delete")]
        [LogOperate(Title = "删除系统参数", BusinessType = (int)OperateType.Delete)]
        public ActionResult Delete(int id)
        {
            try
            {
                _configService.DeleteBy(x => x.Id == id);
                return WriteSuccess("数据删除成功");
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }        
        #endregion

        #region 获取数据
        [CheckPermission("system.config.list")]
        public JsonResult GetList(SysConfigModel viewModel, PageInfo pageInfo)
        {
            QueryCollection queries = new QueryCollection();
            if (!string.IsNullOrEmpty(viewModel.ConfigName))
            {
                queries.Add(new Query { Name = "ConfigName", Operator = Query.Operators.Contains, Value = viewModel.ConfigName });
            }
            if (!string.IsNullOrEmpty(viewModel.ConfigKeys))
            {
                queries.Add(new Query { Name = "ConfigKeys", Operator = Query.Operators.Contains, Value = viewModel.ConfigKeys });
            }
         
            var list = _configService.GetListByPage(queries.AsExpression<SysConfigEntity>(), pageInfo.Field, pageInfo.Order, pageInfo.Limit, pageInfo.Page, out int totalCount).Select(x => new
            {
                x.Id,
                x.ConfigName,
                x.ConfigKeys,
                x.ConfigValue,
                x.ConfigType,
                x.Remark,
                x.CreateTime
            }).ToList();
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}