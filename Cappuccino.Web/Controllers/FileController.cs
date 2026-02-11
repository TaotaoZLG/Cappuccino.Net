using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Cappuccino.Common.Extensions;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;
using MiniExcelLibs;

namespace Cappuccino.Web.Controllers
{
    public class FileController : BaseController
    {
        private const string ExcelUploadPath = "~/Resource/Upload/Excel";
        private readonly List<string> _allowedExcelExtensions = new List<string> { ".xlsx", ".xls", ".csv" };

        /// <summary>
        /// 上传Excel文件并解析内容
        /// </summary>
        [HttpPost]
        public ActionResult UploadExcel()
        {
            var uploadFile = new UploadFile();
            try
            {
                var file = Request.Files[0];
                if (file == null || file.ContentLength == 0)
                {
                    uploadFile.Status = -1;
                    uploadFile.Message = "请选择有效的文件";
                    return Json(uploadFile, JsonRequestBehavior.AllowGet);
                }

                // 验证文件类型
                var fileExt = Path.GetExtension(file.FileName)?.ToLower();
                if (!_allowedExcelExtensions.Contains(fileExt))
                {
                    uploadFile.Status = -1;
                    uploadFile.Message = $"不支持的文件类型，允许的类型：{string.Join(",", _allowedExcelExtensions)}";
                    return Json(uploadFile, JsonRequestBehavior.AllowGet);
                }

                // 准备保存路径
                var localPath = Server.MapPath(ExcelUploadPath);
                if (!Directory.Exists(localPath))
                {
                    Directory.CreateDirectory(localPath);
                }

                // 生成唯一文件名
                var fileName = $"{DateTimeExtensions.CreateNo()}{fileExt}";
                var savePath = Path.Combine(localPath, fileName);
                file.SaveAs(savePath);

                // 解析Excel内容（返回前100行预览）
                var data = MiniExcel.Query(savePath).Take(100).ToList();

                uploadFile.Status = 0;
                uploadFile.Src = Path.Combine(ExcelUploadPath.Replace("~", ""), fileName);
                uploadFile.Message = "上传并解析成功";
                return Json(new { uploadFile, previewData = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                uploadFile.Status = -1;
                uploadFile.Message = $"处理失败：{ex.Message}";
                return Json(uploadFile, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 下载Excel模板/数据
        /// </summary>
        /// <param name="fileName">下载文件名（不含扩展名）</param>
        [HttpGet]
        public ActionResult DownloadExcel<T>(List<T> sampleData, string fileName)
        {
            try
            {
                if (sampleData == null || !sampleData.Any())
                {
                    return WriteError("没有可导出的数据");
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = $"导出数据_{DateTime.Now:yyyyMMddHHmmss}";
                }

                // 处理文件名编码
                //var encodedFileName = HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8);
                //Response.AddHeader("Content-Disposition", $"attachment; filename*=UTF-8''{encodedFileName}.xlsx");

                // 使用MiniExcel流式输出，将内存流转成文件流
                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.SaveAs(sampleData);
                    memoryStream.Seek(0, SeekOrigin.Begin); // 重置流位置
                    return new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"{fileName}.xlsx"
                    };
                }
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }
    }
}