using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Cappuccino.BLL;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Util;
using Cappuccino.IBLL;
using Cappuccino.Web.Core;
using Cappuccino.Web.Hubs;
using Cappuccino.Web.Models;

namespace Cappuccino.Web.Areas.Business.Controllers
{
    public class SysFileProcessiongController : BaseController
    {
        private ISysFileProcessiongService _fileProcessService;

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
        /// 上传并处理压缩包（异步）- 统一处理进度推送
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> UploadAndProcess()
        {
            try
            {
                var file = Request.Files["compressFile"];
                // 基础校验
                if (file == null || file.ContentLength == 0)
                {
                    return WriteError("请选择压缩包文件");
                }

                // 文件大小校验
                var maxFileSize = ConfigUtils.AppSetting.GetValue("UploadMaxFileSize").ParseToInt();
                if (file.ContentLength > maxFileSize)
                {
                    return WriteError($"文件大小超过限制（{maxFileSize / 1024 / 1024}MB）");
                }

                // 格式校验
                var supportCompressFormats = ConfigUtils.AppSetting.GetValue("CompressSupportFormats");
                var fileExt = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
                if (!supportCompressFormats.Contains(fileExt))
                {
                    return WriteError($"不支持的压缩包格式！仅支持：{string.Join(",", supportCompressFormats)}");
                }

                // 1. 创建CancellationToken（支持取消）
                var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;

                // 2. 定义进度回调（核心：Controller层统一推送SignalR）
                var progress = new Progress<ProcessProgress>(p =>
                {
                    // 1. 保存进度日志（可选，如需持久化）
                    // _fileProcessLogDao.Add(...) 

                    // 2. 推送进度到前端（SignalR）- 核心迁移点
                    ProcessProgressHub.SendProgress(p);
                });

                // 3. 调用BLL层业务逻辑（仅传文件、取消令牌、进度回调）
                var batchId = await _fileProcessService.ProcessCompressFileAsync(file, cancellationToken, progress);

                // 4. 返回批次ID给前端
                return WriteSuccess("任务处理成功", batchId);
            }
            catch (Exception ex)
            {
                // 异常进度推送
                ProcessProgressHub.SendProgress(new ProcessProgress
                {
                    BatchId = Guid.NewGuid().ToString(),
                    Type = "Error",
                    Progress = 0,
                    Message = $"任务启动失败：{ex.Message}"
                });
                return WriteError("任务启动失败：" + ex.Message);
            }
        }
        #endregion
    }
}