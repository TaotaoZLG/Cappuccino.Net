using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Log;
using Cappuccino.Common.Util;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;
using static Cappuccino.Common.Helper.AIRecognitionHelper;

namespace Cappuccino.BLL
{
    public class SysFileProcessiongService : BaseService<SysCaseInfoEntity>, ISysFileProcessiongService
    {
        private readonly ISysFileProcessiongDao _fileProcessiongDao;
        private readonly ISysFileDao _fileDao;

        #region 依赖注入
        public SysFileProcessiongService(ISysFileProcessiongDao fileProcessiongDao, ISysFileDao fileDao)
        {
            _fileProcessiongDao = fileProcessiongDao;
            _fileDao = fileDao;
            base.CurrentDao = fileProcessiongDao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

        /// <summary>
        /// 处理压缩包图片（解压→归档→OCR→生成Excel→打包→清理）
        /// </summary>
        /// <param name="compressFilePath">已保存的压缩包物理路径</param>
        /// <param name="extractRule">归档规则</param>
        /// <param name="batchId">批次ID</param>
        /// <param name="progressAction">进度回调</param>
        /// <returns>导出ZIP路径</returns>
        public async Task<string> ProcessCompressFileAsync(string compressFilePath, int extractRule, string batchId, Action<ProcessProgress> progressAction)
        {
            // 1. 从配置文件获取路径（保持为虚拟路径，不再这里转物理路径）
            string tempRootPath = ConfigUtils.AppSetting.GetValue("TempRootPath");
            string exportRootPath = ConfigUtils.AppSetting.GetValue("ExportRootPath");
            string archiveRootPath = ConfigUtils.AppSetting.GetValue("ArchiveRootPath");

            // 重新定义用于 IO 操作的物理路径变量 (基于原代码逻辑补充)
            string tempRootDir = Path.Combine(FileHelper.GetPhysicalPath(tempRootPath), batchId);
            string unzipDir = Path.Combine(tempRootDir, "unzip");
            string archiveDir = Path.Combine(FileHelper.GetPhysicalPath(archiveRootPath), batchId);
            string exportRootDir_Physical = Path.Combine(FileHelper.GetPhysicalPath(exportRootPath), batchId); // 物理路径用于IO

            // 定义用于传递给新方法的虚拟路径
            string exportRootDir_Virtual = Path.Combine(exportRootPath, batchId);
            string finalZipFileName = string.Format("压缩包处理结果_{0}.zip", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            string finalZipPath_Virtual = Path.Combine(exportRootPath, string.Format("final_{0}", finalZipFileName)); // 虚拟路径

            // 统一创建所有批次目录 (使用物理路径)
            FileHelper.EnsureDirectoryExists(tempRootDir);
            FileHelper.EnsureDirectoryExists(unzipDir);
            FileHelper.EnsureDirectoryExists(archiveDir);
            FileHelper.EnsureDirectoryExists(exportRootDir_Physical);

            try
            {
                // 3. 解压压缩包 (compressFilePath 是物理路径，unzipDir 是物理路径)
                List<string> unzipFiles = await CompressHelper.UnzipFileAsync(compressFilePath, unzipDir, batchId, progressAction).ConfigureAwait(false);

                // 4. 按规则归档文件 (unzipFiles 是物理路径列表，archiveDir 是物理路径)
                FileArchiveRuleEnum rule = (FileArchiveRuleEnum)extractRule;
                Dictionary<string, List<string>> archiveDict = await CompressHelper.ArchiveFilesAsync(unzipFiles, archiveDir, rule, batchId, progressAction).ConfigureAwait(false);

                // 5. 过滤图片并OCR识别 (imagePaths 是物理路径)
                Dictionary<string, Dictionary<string, string>> ocrResultDict = new Dictionary<string, Dictionary<string, string>>();
                foreach (var folderItem in archiveDict)
                {
                    string folderName = folderItem.Key;
                    List<string> filePaths = folderItem.Value;

                    List<string> imageFiles = await CompressHelper.FilterImageFilesAsync(filePaths, batchId, progressAction).ConfigureAwait(false);
                    if (imageFiles.Count == 0)
                    {
                        progressAction.Invoke(new ProcessProgress
                        {
                            BatchId = batchId,
                            Progress = 0,
                            Type = "Filter",
                            Message = $"文件夹{folderName}无有效图片，跳过OCR识别"
                        });
                        continue;
                    }

                    Dictionary<string, string> ocrResults = new Dictionary<string, string>();
                    foreach (var imagePath in imageFiles)
                    {
                        string ocrText = await AIRecognitionHelper.ImageOcrRecognizeAsync(imagePath, batchId, progressAction).ConfigureAwait(false);
                        ocrResults.Add(imagePath, ocrText);

                        long Id = IdGeneratorHelper.Instance.NextId();
                       
                        // 识别结果入库
                        Insert(new SysCaseInfoEntity
                        {
                            Id = Id,
                            CustName = "Cappuccino客户",
                            CustIDNumber = "",
                            BatchId = batchId,
                            Remark1 = "测试"
                        });
                    }
                    ocrResultDict.Add(folderName, ocrResults);
                }

                // 6. 生成Excel识别结果 (excelPath 需要是物理路径以便写入文件)
                string excelFileName = string.Format("OCR识别结果_{0}.xlsx", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                string excelPath_Physical = Path.Combine(exportRootDir_Physical, excelFileName);
                await CompressHelper.GenerateOcrExcelAsync(ocrResultDict, excelPath_Physical, batchId, progressAction).ConfigureAwait(false);

                // 7. 复制归档文件夹到导出目录 (使用物理路径)
                foreach (var folderItem in archiveDict)
                {
                    string sourceFolder = Path.Combine(archiveDir, folderItem.Key);
                    string targetFolder = Path.Combine(exportRootDir_Physical, folderItem.Key);
                    CompressHelper.DirectoryCopy(sourceFolder, targetFolder, true);
                }

                // 8. 【修改点】打包导出目录为最终ZIP
                // 只传虚拟路径给 PackFolderToZipAsync
                await CompressHelper.PackFolderToZipAsync(exportRootDir_Virtual, finalZipPath_Virtual, batchId, progressAction).ConfigureAwait(false);

                // 9. 清理临时目录
                await CompressHelper.CleanTempDirAsync(tempRootDir, batchId, progressAction).ConfigureAwait(false);

                return finalZipPath_Virtual;
            }
            catch (Exception ex)
            {
                progressAction.Invoke(new ProcessProgress
                {
                    BatchId = batchId,
                    Progress = 0,
                    Type = "Error",
                    Message = $"处理失败：{ex.Message}"
                });
                await CompressHelper.CleanTempDirAsync(tempRootDir, batchId, progressAction);
                throw;
            }
        }
    }
}