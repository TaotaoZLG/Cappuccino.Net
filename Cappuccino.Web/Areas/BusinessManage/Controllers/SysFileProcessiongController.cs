using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Util;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.Web.Core;
using MiniExcelLibs;

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
        public async Task<ActionResult> UploadAndProcess(HttpPostedFileBase compressFile, int extractRule, int processType)
        {
            TData<string> result = new TData<string>();
            string batchId = GuidHelper.GetGuid(true);
            try
            {
                // 处理进度回调
                Action<ProcessProgress> progressAction = (progress) =>
                {
                    ProcessProgressHub.SendProgress(progress);
                };

                result = await _fileProcessService.ProcessCompressFileAsync(compressFile, extractRule, processType, batchId, progressAction).ConfigureAwait(false);
                if (result.Status == 0)
                {
                    return WriteError(result.Message);
                }

                // 推送完成消息
                ProcessProgressHub.SendProgress(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 100,
                    Type = "Finish",
                    Message = $"处理完成，可下载文件：{result.Data}"
                });
                return WriteSuccess("任务处理成功", new { data = result.Data, BatchId = batchId });
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