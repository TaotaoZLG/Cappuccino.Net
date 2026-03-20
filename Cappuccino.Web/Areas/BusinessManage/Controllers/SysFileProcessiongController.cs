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
                // 获取上传文件
                HttpPostedFileBase file = Request.Files["compressFile"];

                // 获取归档规则
                if (!int.TryParse(Request.Form["extractRule"], out int extractRule))
                {
                    extractRule = (int)FileArchiveRuleEnum.SystemDefault;
                }

                // 处理进度回调
                Action<ProcessProgress> progressAction = (progress) =>
                {
                    ProcessProgressHub.SendProgress(progress);
                };

                var result = await _fileProcessService.ProcessCompressFileAsync(file, extractRule, batchId, progressAction).ConfigureAwait(false);
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