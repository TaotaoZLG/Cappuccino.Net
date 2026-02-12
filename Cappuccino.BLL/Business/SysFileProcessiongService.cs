using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Log;
using Cappuccino.Common.Util;
using Cappuccino.Entity;
using Cappuccino.Entity.Business;
using Cappuccino.IBLL;
using Cappuccino.IDAL;

namespace Cappuccino.BLL
{
    public class SysFileProcessiongService : BaseService<SysCaseInfoEntity>, ISysFileProcessiongService
    {
        private ISysFileProcessiongDao _fileProcessiongDao;

        #region 依赖注入
        public SysFileProcessiongService(ISysFileProcessiongDao fileProcessiongDao)
        {
            _fileProcessiongDao = fileProcessiongDao;
            base.CurrentDao = fileProcessiongDao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

        /// <summary>
        /// 批量处理压缩包（异步）- 仅返回进度，不推送SignalR
        /// 执行完整流程：解压→过滤→AI识别→入库→清理
        /// </summary>
        /// <param name="file">上传的压缩包文件</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="progress">进度回调（由Controller传入）</param>
        /// <returns>处理结果</returns>
        public async Task<string> ProcessCompressFileAsync(HttpPostedFileBase file, CancellationToken cancellationToken, IProgress<ProcessProgress> progress)
        {
            // 1. 基础校验
            var batchId = GuidHelper.GetGuid(true);
            var supportCompressFormats = ConfigUtils.AppSetting.GetValue("CompressSupportFormats");

            // 2. 定义路径
            var CompressTempPath = ConfigUtils.AppSetting.GetValue("CompressTempPath");
            var UnzipTempPath = ConfigUtils.AppSetting.GetValue("UnzipTempPath");
            var ValidFilePath = ConfigUtils.AppSetting.GetValue("ValidFilePath");
            var uploadTempDir = FileHelper.GetPhysicalPath(CompressTempPath);
            var unzipTempDir = FileHelper.GetPhysicalPath(UnzipTempPath) + batchId + "\\";
            var validImageDir = FileHelper.GetPhysicalPath(ValidFilePath);

            // 3. 保存上传的压缩包
            var fileExt = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
            var zipPath = Path.Combine(uploadTempDir, $"{batchId}.{fileExt}");
            file.SaveAs(zipPath);

            try
            {
                // 4. 解压压缩包（异步）- 进度通过回调返回
                progress.Report(new ProcessProgress
                {
                    BatchId = batchId,
                    Type = "Unzip",
                    Progress = 0,
                    Message = "开始解压压缩包..."
                });
                await CompressHelper.UnzipFileAsync(zipPath, unzipTempDir, batchId, progress);

                // 5. 过滤有效图片
                progress.Report(new ProcessProgress
                {
                    BatchId = batchId,
                    Type = "Filter",
                    Progress = 0,
                    Message = "开始过滤有效图片..."
                });
                var validImages = CompressHelper.FilterValidImages(unzipTempDir, batchId, progress);

                // 6. AI识别 + 入库 + 移动有效图片
                progress.Report(new ProcessProgress
                {
                    BatchId = batchId,
                    Type = "Recognize",
                    Progress = 0,
                    Message = $"共识别到{validImages.Count}张有效图片..."
                });
                var total = validImages.Count;
                var processed = 0;
                foreach (var image in validImages)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    processed++;

                    // AI识别
                    AIRecognitionHelper.AIRecognitionResult aiResult = await AIRecognitionHelper.RecognizeImageAsync(image, batchId, progress);
                    if (!aiResult.Success)
                    {
                        progress.Report(new ProcessProgress
                        {
                            BatchId = batchId,
                            Type = "Recognize",
                            Progress = (int)((processed / (double)total) * 100),
                            Message = $"识别失败：{image}，原因：{aiResult.Message}"
                        });
                        continue;
                    }

                    // 移动有效图片
                    var targetPath = CompressHelper.MoveValidImage(image, validImageDir);

                    string FilePath = aiResult.ImageType;
                    string AIRecognitionResult = aiResult.ToString();

                    // 识别记录入库

                    // 识别结果入库
                    Insert(new SysCaseInfoEntity
                    {
                        BatchId = batchId,
                        Remark1 = AIRecognitionResult
                    });

                    // 反馈识别进度（核心：仅返回进度数据，不推送SignalR）
                    progress.Report(new ProcessProgress
                    {
                        BatchId = batchId,
                        Type = "Recognize",
                        Progress = (int)((processed / (double)total) * 100),
                        Message = $"已处理 {processed}/{total} 张图片，识别类型：{aiResult.ImageType}"
                    });
                }

                // 7. 清理临时文件
                progress.Report(new ProcessProgress
                {
                    BatchId = batchId,
                    Type = "Clean",
                    Progress = 0,
                    Message = "开始清理临时文件..."
                });
                CompressHelper.CleanTempFiles(unzipTempDir, batchId, progress);
                File.Delete(zipPath); // 删除上传的压缩包

                // 处理完成
                progress.Report(new ProcessProgress
                {
                    BatchId = batchId,
                    Type = "Finish",
                    Progress = 100,
                    Message = $"批量处理完成，共识别有效图片 {validImages.Count} 张"
                });
            }
            catch (Exception ex)
            {
                // 异常处理
                progress.Report(new ProcessProgress
                {
                    BatchId = batchId,
                    Type = "Error",
                    Progress = 0,
                    Message = $"批量处理失败：{ex.Message}"
                });

                Log4netHelper.Error($"批量处理失败，BatchId={batchId}，错误信息：{ex}");
            }
            return batchId;
        }
    }
}