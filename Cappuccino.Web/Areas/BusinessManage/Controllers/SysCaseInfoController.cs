using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Util;
using Cappuccino.DataAccess;
using Cappuccino.Model;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;

namespace Cappuccino.Web.Areas.BusinessManage.Controllers
{
    public class SysCaseInfoController : BaseController
    {
        #region 视图
        [CheckPermission("business.case.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }
        #endregion

        #region 提交数据
        /// <summary>
        /// 上传文件
        /// </summary>
        [HttpPost]
        [CheckPermission("system.case.uploadfile")]
        public async Task<ActionResult> UploadFile()
        {
            try
            {
                var file = Request.Files["fileList"];
                

                // 文件大小校验
                var maxFileSize = ConfigUtils.AppSetting.GetValue("UploadMaxFileSize").ParseToInt();
                if (file.ContentLength > maxFileSize)
                {
                    return WriteError($"文件大小超过限制（{maxFileSize / 1024 / 1024}MB）");
                }


                return WriteSuccess("任务处理成功");
            }
            catch (Exception ex)
            {
                return WriteError("任务启动失败：" + ex.Message);
            }
        }
        #endregion

        #region 获取数据
        [CheckPermission("system.case.list")]
        public JsonResult GetList(SysCaseInfoModel viewModel, PageInfo pageInfo)
        {
            var coreSql = $@"
                SELECT * FROM dbo.SysCaseInfo c
                WHERE 1=1
                {(!string.IsNullOrEmpty(viewModel.CustIDNumber) ? $"AND c.CustIDNumber LIKE '%{viewModel.CustIDNumber}%'" : "")}
                {(!string.IsNullOrEmpty(viewModel.CustName) ? $"AND c.CustName LIKE '%{viewModel.CustName}%'" : "")}
            ";

            var list = DapperHelper.QueryPage<dynamic>(coreSql, pageInfo.Field, pageInfo.Order, pageInfo.Limit, pageInfo.Page, out int totalCount);
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}