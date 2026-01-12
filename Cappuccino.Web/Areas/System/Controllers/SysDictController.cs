using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Description;
using Cappuccino.BLL;
using Cappuccino.Common;
using Cappuccino.Common.Enum;
using Cappuccino.Common.Util;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.Model;
using Cappuccino.Web.Attributes;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysDictController : BaseController
    {
        private readonly ISysDictService _sysDictService;
        private readonly ISysDictDetailService _sysDictDetailService;

        public SysDictController(ISysDictService sysDictService, ISysDictDetailService sysDictDetailService)
        {
            _sysDictService = sysDictService;
            _sysDictDetailService = sysDictDetailService;
            this.AddDisposableObject(_sysDictService);
            this.AddDisposableObject(_sysDictDetailService);
        }

        #region 视图
        [CheckPermission("system.dict.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }


        [HttpGet, CheckPermission("system.dict.create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet, CheckPermission("system.dict.edit")]
        public ActionResult Edit(int id)
        {
            var viewModel = _sysDictService.GetList(x => x.Id == id).FirstOrDefault();
            return View(viewModel.EntityMap());
        }
        #endregion

        #region 提交数据
        [HttpPost, CheckPermission("system.dict.create")]
        [LogOperate(Title = "新增字典", BusinessType = (int)OperateType.Add)]
        public ActionResult Create(SysDictModel viewModel)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return WriteError("实体验证失败");
                }
                SysDictEntity entity = viewModel.EntityMap();
                entity.CreateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.CreateTime = DateTime.Now;
                entity.UpdateTime = DateTime.Now;
                _sysDictService.Insert(entity);
                return WriteSuccess();
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.dict.edit")]
        [LogOperate(Title = "编辑字典", BusinessType = (int)OperateType.Update)]
        public ActionResult Edit(SysDictModel viewModel)
        {
            if (ModelState.IsValid == false)
            {
                return WriteError("实体验证失败");
            }
            viewModel.Id = viewModel.Id;
            viewModel.UpdateTime = DateTime.Now;
            viewModel.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
            SysDictEntity entity = viewModel.EntityMap();
            _sysDictService.Update(entity, new string[] { "Name", "Code", "SortCode", "UpdateTime", "UpdateUserId" });
            return WriteSuccess();
        }

        [HttpPost, CheckPermission("system.dict.delete")]
        [LogOperate(Title = "删除字典", BusinessType = (int)OperateType.Delete)]
        public ActionResult Delete(int id)
        {
            try
            {
                _sysDictService.DeleteBy(x => x.Id == id);
                return WriteSuccess("数据删除成功");
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.dict.batchDel")]
        [LogOperate(Title = "批量删除字典", BusinessType = (int)OperateType.Delete)]
        public ActionResult BatchDel(string idsStr)
        {
            try
            {
                var idsArray = idsStr.Substring(0, idsStr.Length).Split(',');
                int[] ids = Array.ConvertAll<string, int>(idsArray, int.Parse);
                var result = _sysDictService.DeleteBy(x => ids.Contains(x.Id)) > 0 ? WriteSuccess("数据删除成功") : WriteError("数据删除失败");
                return result;
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }
        #endregion

        #region 获取数据
        [CheckPermission("system.dict.list")]
        public JsonResult GetList(SysDictModel viewModel, PageInfo pageInfo)
        {
            QueryCollection queries = new QueryCollection();
            if (!string.IsNullOrEmpty(viewModel.Name))
            {
                queries.Add(new Query { Name = "Name", Operator = Query.Operators.Contains, Value = viewModel.Name });

            }
            else if (!string.IsNullOrEmpty(viewModel.Code))
            {
                queries.Add(new Query { Name = "Code", Operator = Query.Operators.Contains, Value = viewModel.Code });
            }
            var list = _sysDictService.GetListByPage(queries.AsExpression<SysDictEntity>(), pageInfo.Field, pageInfo.Order, pageInfo.Limit, pageInfo.Page, out int totalCount).Select(x => new
            {
                x.Id,
                x.Name,
                x.Code,
                x.SortCode,
                x.CreateTime
            }).ToList();
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, CheckPermission("system.dict.list")]
        public JsonResult GetDataDictList()
        {
            // 获取所有字典类型
            List<SysDictEntity> dictList = _sysDictService.GetList(x => true).ToList();
            // 按类型Code分组，关联字典项
            var result = dictList.Select(type => new
            {
                TypeCode = type.Code,  // 字典类型编码
                TypeName = type.Name,  // 字典类型名称
                Dicts = _sysDictDetailService.GetList(d => d.DictId == type.Id)
                    .Select(d => new
                    {
                        Label = d.Name,  // 字典项名称
                        Value = d.Code,  // 字典项值
                        Sort = d.SortCode,  // 排序号
                        Class = d.ListClass  // 显示样式
                    })
                    .OrderBy(d => d.Sort)
                    .ToList()
            }).ToDictionary(x => x.TypeCode);

            return Json(new { Status = 0, Data = result, Message = "查询成功" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMaxSortCode()
        {
            int maxSortCode = _sysDictService.GetMaxSortCode();
            var result = new { Status = 0, Message = "查询成功", Data = maxSortCode };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}