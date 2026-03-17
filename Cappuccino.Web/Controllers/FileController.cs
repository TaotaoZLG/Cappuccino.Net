using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Util;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;
using Microsoft.Owin;
using MiniExcelLibs;

namespace Cappuccino.Web.Controllers
{
    public class FileController : BaseController
    {
        #region 视图
        public ActionResult UploadFile()
        {
            return PartialView();
        }
        #endregion

        #region 上传单个文件
        [HttpPost]
        [CheckPermission]
        public async Task<ActionResult> UploadFile(int fileModule)
        {
            HttpPostedFileBase file = Request.Files[0];
            TData<string> obj = await FileHelper.UploadFile(fileModule, file);
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 删除单个文件
        [HttpPost]
        [CheckPermission]
        public ActionResult DeleteFile(int fileModule, string filePath)
        {
            TData<string> obj = FileHelper.DeleteFile(fileModule, filePath);
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 下载文件
        /// <summary>
        /// 下载文件（如Excel模板或处理结果），下载后可选择删除服务器上的文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="delete"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet]
        public FileContentResult DownloadFile(string filePath, int delete = 1)
        {
            TData<FileContentResult> obj = FileHelper.DownloadFile(filePath, delete);
            if (obj.Status == 1)
            {
                return obj.Data;
            }
            else
            {
                throw new Exception("下载失败：" + obj.Message);
            }
        }
        #endregion
    }
}