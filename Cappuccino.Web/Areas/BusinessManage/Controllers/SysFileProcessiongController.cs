using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Util;
using Cappuccino.IBLL;
using Cappuccino.Web.Core;

namespace Cappuccino.Web.Areas.BusinessManage.Controllers
{
    public class SysFileProcessiongController : BaseController
    {
        private readonly ISysFileProcessiongService _fileProcessService;

        public SysFileProcessiongController(ISysFileProcessiongService fileProcessService)
        {
            _fileProcessService = fileProcessService;
            this.AddDisposableObject(_fileProcessService);
        }

        #region 视图
        [CheckPermission("business.fileprocessor.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }
        #endregion

        #region 提交数据
        /// <summary>
        /// 上传并处理压缩包
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> UploadAndProcess()
        {
            string batchId = GuidHelper.GetGuid(true);
            try
            {
                // 1. 获取上传文件
                HttpPostedFileBase file = Request.Files["compressFile"];
                if (file == null || file.ContentLength == 0)
                {
                    return WriteError("请选择要上传的压缩包文件");
                }

                // 2. 新增：文件格式校验（修复前端「不支持该文件类型」报错）
                string fileName = file.FileName;
                string fileExt = Path.GetExtension(fileName).TrimStart('.').ToLower();
                string supportFormats = ConfigUtils.AppSetting.GetValue("CompressSupportFormats");
                var supportExtList = supportFormats.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().ToLower()).ToList();
                if (!supportExtList.Contains(fileExt))
                {
                    return WriteError($"当前不支持该文件类型，请尝试其他文件。支持格式：{string.Join("、", supportExtList)}");
                }

                // 3. 新增：文件大小校验
                long maxSize = ConfigUtils.AppSetting.GetValue("UploadMaxFileSize").ParseToLong();
                if (file.ContentLength > maxSize)
                {
                    return WriteError($"文件大小超出限制，最大支持{maxSize / 1024 / 1024}MB");
                }

                // 4. 修复：同步保存文件到临时目录，避免异步线程中HttpContext释放导致文件对象失效
                string tempRootVirtualPath = ConfigUtils.AppSetting.GetValue("TempRootPath");
                string fileVirtualPath = Path.Combine(tempRootVirtualPath, batchId, fileName);
                string tempCompressPath = FileHelper.GetPhysicalPath(fileVirtualPath);
                FileHelper.EnsureDirectoryExists(tempCompressPath);
                file.SaveAs(tempCompressPath); // 同步保存，确保文件内容完整写入

                // 5. 获取归档规则
                if (!int.TryParse(Request.Form["extractRule"], out int extractRule))
                {
                    extractRule = (int)FileArchiveRuleEnum.SystemDefault;
                }

                // 6. 处理进度回调
                Action<ProcessProgress> progressAction = (progress) =>
                {
                    ProcessProgressHub.SendProgress(progress);
                };

                // 7. 直接await调用业务层，移除无效的Task.Run嵌套，避免线程池浪费
                string finalZipPath = await _fileProcessService.ProcessCompressFileAsync(tempCompressPath, extractRule, batchId, progressAction).ConfigureAwait(false);

                // 8. 推送完成消息
                ProcessProgressHub.SendProgress(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 100,
                    Type = "Finish",
                    Message = $"处理完成，可下载文件：{finalZipPath}"
                });

                // 9. 返回批次ID
                return WriteSuccess("任务处理成功", new { data = finalZipPath, BatchId = batchId });
            }
            catch (Exception ex)
            {
                // 异常进度推送
                ProcessProgressHub.SendProgress(new ProcessProgress
                {
                    BatchId = batchId,
                    Type = "Error",
                    Progress = 0,
                    Message = $"处理失败：{ex.Message}"
                });
                return WriteError("处理失败：" + ex.Message);
            }
        }
        #endregion
    }
}