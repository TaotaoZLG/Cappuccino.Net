using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Cappuccino.Common;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Util;
using Cappuccino.DataAccess;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.Model;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;
using static Cappuccino.Common.Net.NetHelper;

namespace Cappuccino.Web.Areas.BusinessManage.Controllers
{
    public class SysCaseInfoController : BaseController
    {
        private ISysCaseInfoService _sysCaseInfoService;
        private ISysTemplateService _sysTemplateService;

        public SysCaseInfoController(ISysCaseInfoService sysCaseInfoService, ISysTemplateService sysTemplateService )
        {
            _sysCaseInfoService = sysCaseInfoService;
            _sysTemplateService = sysTemplateService;
            this.AddDisposableObject(_sysCaseInfoService);
        }

        #region 视图
        [CheckPermission("business.case.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }

        public ActionResult Indictment()
        {
            return View();
        }

        public ActionResult UploadFile()
        {
            return View();
        }
        #endregion

        #region 提交数据
        /// <summary>
        /// 上传文件
        /// </summary>
        [HttpPost]
        [CheckPermission("system.case.uploadfile")]
        public async Task<ActionResult> UploadFileJson(string saveDirectoryName)
        {
            try
            {
                //HttpFileCollectionBase files = Request.Files;
                //foreach (var item in files.AllKeys)
                //{
                //    var file = files[item];
                //    if (file != null && file.ContentLength > 0)
                //    {
                //        // 处理文件上传
                //    }
                //}
                HttpPostedFileBase fileBase = Request.Files[0];
                if (fileBase == null || fileBase.ContentLength == 0)
                {
                    return WriteError("请选择要上传的压缩包文件");
                }

                string supportFormats = ConfigUtils.AppSetting.GetValue("CompressedFileFormats");
                if (!FileHelper.IsValidFileExtension(fileBase.FileName, supportFormats, '|'))
                {
                    return WriteError($"当前不支持该文件类型，请尝试其他文件。支持格式：{supportFormats}");
                }

                int maxSize = ConfigUtils.AppSetting.GetValue("UploadMaxFileSize").ParseToInt();
                if (fileBase.ContentLength > maxSize)
                {
                    return WriteError($"文件大小超出限制，最大支持{maxSize / 1024 / 1024}MB");
                }

                var obj = await _sysCaseInfoService.UploadFiles(fileBase, saveDirectoryName);
                return WriteJson(obj);
            }
            catch (Exception ex)
            {
                return WriteError("任务上传文件：" + ex.Message);
            }
        }

        /// <summary>
        /// 生成诉状
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> IndictmentJson(SysCaseInfoModel viewModel, string idsStr, long templateId)
        {
            try
            {
                var queries = BuildUserQueries(viewModel);

                if (!string.IsNullOrEmpty(idsStr))
                {
                    var ids = idsStr.Split(',');
                    queries.Add(new Query { Name = "Id", Operator = Query.Operators.In, Value = ids });
                }

                var caseInfoList = _sysCaseInfoService.GetList(queries.AsExpression<SysCaseInfoEntity>()).ToList();

                var data = await _sysCaseInfoService.IndictmentAsync(caseInfoList, templateId).ConfigureAwait(false);
                return WriteJson(data);
            }
            catch (Exception ex)
            {
                return WriteError("起诉书生成失败：" + ex.Message);
            }
        }
        #endregion

        #region 获取数据
        [CheckPermission("system.case.list")]
        public JsonResult GetList(SysCaseInfoModel viewModel, PageInfo pageInfo)
        {
            //var coreSql = $@"
            //    SELECT * FROM dbo.SysCaseInfo c
            //    WHERE 1=1
            //    {(!string.IsNullOrEmpty(viewModel.CustIDNumber) ? $"AND c.CustIDNumber LIKE '%{viewModel.CustIDNumber}%'" : "")}
            //    {(!string.IsNullOrEmpty(viewModel.CustName) ? $"AND c.CustName LIKE '%{viewModel.CustName}%'" : "")}
            //";
            //var list = DapperHelper.QueryPage<dynamic>(coreSql, pageInfo.Field, pageInfo.Order, pageInfo.Limit, pageInfo.Page, out int totalCount);
            //return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);

            var queries = BuildUserQueries(viewModel);
            var list = _sysCaseInfoService.GetListByPage(queries.AsExpression<SysCaseInfoEntity>(), pageInfo.Field, pageInfo.Order, pageInfo.Limit, pageInfo.Page, out int totalCount).ToList();
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 复用查询条件构建逻辑（与GetList统一，避免重复代码）
        /// </summary>
        private QueryCollection BuildUserQueries(SysCaseInfoModel viewModel)
        {
            QueryCollection queries = new QueryCollection();
            if (!string.IsNullOrEmpty(viewModel.CustIDNumber))
            {
                queries.Add(new Query { Name = "CustIDNumber", Operator = Query.Operators.Contains, Value = viewModel.CustIDNumber });
            }
            if (!string.IsNullOrEmpty(viewModel.CustName))
            {
                queries.Add(new Query { Name = "CustName", Operator = Query.Operators.Equal, Value = viewModel.CustName });
            }
            return queries;
        }
        #endregion
    }
}