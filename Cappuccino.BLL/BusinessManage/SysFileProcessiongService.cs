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
        /// 处理压缩包图片（保存文件→解压→归档→OCR→生成Excel→打包→清理）
        /// </summary>
        /// <param name="file">上传的压缩包文件</param>
        /// <param name="extractRule">归档规则</param>
        /// <param name="batchId">批次ID</param>
        /// <param name="progressAction">进度回调</param>
        /// <returns>导出ZIP路径</returns>
        public async Task<TData<string>> ProcessCompressFileAsync(HttpPostedFileBase file, int extractRule, string batchId, Action<ProcessProgress> progressAction)
        {
            TData<string> obj = new TData<string>();
            try
            {
                if (file == null || file.ContentLength == 0)
                {
                    obj.Status = 0;
                    obj.Message = "请选择要上传的压缩包文件";
                    return obj;
                }

                // 文件格式校验
                string fileName = file.FileName;
                string fileExt = Path.GetExtension(fileName).TrimStart('.').ToLower();
                string supportFormats = ConfigUtils.AppSetting.GetValue("CompressSupportFormats");
                var supportExtList = supportFormats.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim().ToLower()).ToList();
                if (!supportExtList.Contains(fileExt))
                {
                    obj.Status = 0;
                    obj.Message = $"当前不支持该文件类型，请尝试其他文件。支持格式：{string.Join("、", supportExtList)}";
                    return obj;
                }

                // 3. 文件大小校验
                long maxSize = ConfigUtils.AppSetting.GetValue("UploadMaxFileSize").ParseToLong();
                if (file.ContentLength > maxSize)
                {
                    obj.Status = 0;
                    obj.Message = $"文件大小超出限制，最大支持{maxSize / 1024 / 1024}MB";
                    return obj;
                }

                // 1. 从配置文件获取路径
                string tempRootPath = ConfigUtils.AppSetting.GetValue("TempRootPath");
                string exportRootPath = ConfigUtils.AppSetting.GetValue("ExportRootPath");
                string archiveRootPath = ConfigUtils.AppSetting.GetValue("ArchiveRootPath");

                // 2. 保存上传文件
                string fileVirtualPath = Path.Combine(tempRootPath, batchId, fileName);
                string tempCompressPath = FileHelper.GetPhysicalPath(fileVirtualPath);
                FileHelper.EnsureDirectoryExists(Path.GetDirectoryName(tempCompressPath));
                file.SaveAs(tempCompressPath); // 同步保存，确保文件内容完整写入

                // 3. 定义用于 IO 操作的物理路径变量
                string tempRootDir = Path.Combine(FileHelper.GetPhysicalPath(tempRootPath), batchId);
                string unzipDir = Path.Combine(tempRootDir, "unzip");
                string archiveDir = Path.Combine(FileHelper.GetPhysicalPath(archiveRootPath), batchId);
                string exportRootDir_Physical = Path.Combine(FileHelper.GetPhysicalPath(exportRootPath), batchId);

                // 定义用于传递给新方法的虚拟路径
                string exportRootDir_Virtual = Path.Combine(exportRootPath, batchId);
                string finalZipFileName = string.Format("压缩包处理结果_{0}.zip", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                string finalZipPath_Virtual = Path.Combine(exportRootPath, string.Format("final_{0}", finalZipFileName));

                // 4. 统一创建所有批次目录
                FileHelper.EnsureDirectoryExists(tempRootDir);
                FileHelper.EnsureDirectoryExists(unzipDir);
                FileHelper.EnsureDirectoryExists(archiveDir);
                FileHelper.EnsureDirectoryExists(exportRootDir_Physical);

                // 5. 解压压缩包
                List<string> unzipFiles = await CompressHelper.UnzipFileAsync(tempCompressPath, unzipDir, batchId, progressAction).ConfigureAwait(false);

                // 6. 按规则归档文件
                FileArchiveRuleEnum rule = (FileArchiveRuleEnum)extractRule;
                Dictionary<string, List<string>> archiveDict = await CompressHelper.ArchiveFilesAsync(unzipFiles, archiveDir, rule, batchId, progressAction).ConfigureAwait(false);

                // 7. 过滤图片并OCR识别
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

                // 8. 生成Excel识别结果
                string excelFileName = string.Format("OCR识别结果_{0}.xlsx", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                string excelPath_Physical = Path.Combine(exportRootDir_Physical, excelFileName);
                await CompressHelper.GenerateOcrExcelAsync(ocrResultDict, excelPath_Physical, batchId, progressAction).ConfigureAwait(false);

                // 9. 复制归档文件夹到导出目录
                foreach (var folderItem in archiveDict)
                {
                    string sourceFolder = Path.Combine(archiveDir, folderItem.Key);
                    string targetFolder = Path.Combine(exportRootDir_Physical, folderItem.Key);
                    CompressHelper.DirectoryCopy(sourceFolder, targetFolder, true);
                }

                // 10. 打包导出目录为最终ZIP
                await CompressHelper.PackFolderToZipAsync(exportRootDir_Virtual, finalZipPath_Virtual, batchId, progressAction).ConfigureAwait(false);

                // 11. 清理临时目录
                await CompressHelper.CleanTempDirAsync(tempRootDir, batchId, progressAction).ConfigureAwait(false);

                obj.Status = 1;
                obj.Message = $"处理完成";
                obj.Data = finalZipPath_Virtual;
                return obj;
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
            }
            finally
            {
                //await CompressHelper.CleanTempDirAsync(tempRootDir, batchId, progressAction);
            }
            return obj;
        }
    }
}