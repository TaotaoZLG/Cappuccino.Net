using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Helpers;
using Cappuccino.Common.Log;
using Cappuccino.Common.Util;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;
using Cappuccino.Web.Core;

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
        /// 处理压缩包图片/Excel（保存文件→解压→分支处理（Excel解析/OCR）→生成Excel→打包→清理）
        /// </summary>
        /// <param name="file">上传的压缩包文件</param>
        /// <param name="extractRule">归档规则</param>
        /// <param name="processType">处理类型：0=Excel+文件 1=图片+文件</param>
        /// <param name="batchId">批次ID</param>
        /// <param name="progressAction">进度回调</param>
        /// <returns>导出ZIP路径</returns>
        public async Task<TData<string>> ProcessCompressFileAsync(HttpPostedFileBase file, int extractRule, int processType, string batchId, Action<ProcessProgress> progressAction)
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

                // 1. 文件格式/大小校验（原有逻辑保留）
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
                long maxSize = ConfigUtils.AppSetting.GetValue("UploadMaxFileSize").ParseToLong();
                if (file.ContentLength > maxSize)
                {
                    obj.Status = 0;
                    obj.Message = $"文件大小超出限制，最大支持{maxSize / 1024 / 1024}MB";
                    return obj;
                }

                // 2. 路径初始化（原有逻辑保留）
                string tempRootPath = ConfigUtils.AppSetting.GetValue("TempRootPath");
                string exportRootPath = ConfigUtils.AppSetting.GetValue("ExportRootPath");
                // 归档文件路径
                string archiveRootPath = ConfigUtils.AppSetting.GetValue("ArchiveRootPath");
                string fileVirtualPath = Path.Combine(tempRootPath, batchId, fileName);
                string tempCompressPath = FileHelper.GetPhysicalPath(fileVirtualPath);
                FileHelper.EnsureDirectoryExists(Path.GetDirectoryName(tempCompressPath));
                file.SaveAs(tempCompressPath);

                string tempRootDir = Path.Combine(FileHelper.GetPhysicalPath(tempRootPath), batchId);
                string unzipDir = Path.Combine(tempRootDir, "unzip");
                string archiveDir = Path.Combine(FileHelper.GetPhysicalPath(archiveRootPath), batchId);
                string exportRootDir_Physical = Path.Combine(FileHelper.GetPhysicalPath(exportRootPath), batchId);
                string exportRootDir_Virtual = Path.Combine(exportRootPath, batchId);
                string finalZipFileName = string.Format("压缩包处理结果_{0}.zip", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                string finalZipPath_Virtual = Path.Combine(exportRootPath, string.Format("final_{0}", finalZipFileName));

                FileHelper.EnsureDirectoryExists(tempRootDir);
                FileHelper.EnsureDirectoryExists(unzipDir);
                FileHelper.EnsureDirectoryExists(archiveDir);
                FileHelper.EnsureDirectoryExists(exportRootDir_Physical);

                // 3. 解压压缩包
                List<string> unzipFiles = await CompressHelper.UnzipFileAsync(tempCompressPath, unzipDir, batchId, progressAction).ConfigureAwait(false);

                // 4. 按规则归档文件
                FileArchiveRuleEnum rule = (FileArchiveRuleEnum)extractRule;
                Dictionary<string, List<string>> archiveDict = await CompressHelper.ArchiveFilesAsync(unzipFiles, archiveDir, rule, batchId, progressAction).ConfigureAwait(false);

                // 5. 案件/文件信息初始化
                List<SysCaseInfoEntity> caseInfoList = new List<SysCaseInfoEntity>();
                List<SysFileEntity> fileList = new List<SysFileEntity>();
                Dictionary<string, Dictionary<string, string>> ocrResultDict = new Dictionary<string, Dictionary<string, string>>();

                // 6. 分支处理：根据processType选择Excel解析或OCR识别
                if (processType == 0)
                {
                    // 6.1 处理类型0：Excel解析逻辑
                    progressAction.Invoke(new ProcessProgress
                    {
                        BatchId = batchId,
                        Progress = 30,
                        Type = "Filter",
                        Message = "开始解析Excel文件"
                    });

                    // 筛选压缩包内的Excel文件（xlsx/xls）
                    List<string> excelFiles = unzipFiles.Where(f =>
                    {
                        string ext = Path.GetExtension(f).TrimStart('.').ToLower();
                        return ext == "xlsx" || ext == "xls";
                    }).ToList();

                    if (excelFiles.Count == 0)
                    {
                        progressAction.Invoke(new ProcessProgress
                        {
                            BatchId = batchId,
                            Progress = 30,
                            Type = "Error",
                            Message = "压缩包内未找到Excel文件（xlsx/xls）"
                        });
                        obj.Status = 0;
                        obj.Message = "压缩包内未找到Excel文件（xlsx/xls）";
                        return obj;
                    }

                    // 解析Excel文件（MiniExcel）
                    foreach (var excelPath in excelFiles)
                    {
                        try
                        {
                            // 使用MiniExcel读取Excel，映射中文列头到实体
                            var excelData = MiniExcelHelper<SysCaseInfoEntity>.ImportFromExcel(excelPath);
                            if (excelData.Count == 0)
                            {
                                progressAction.Invoke(new ProcessProgress
                                {
                                    BatchId = batchId,
                                    Progress = 40,
                                    Type = "Filter",
                                    Message = $"Excel文件{Path.GetFileName(excelPath)}无有效数据，跳过"
                                });
                                continue;
                            }

                            // 补充实体默认值（如ID、创建时间等）
                            foreach (var item in excelData)
                            {
                                item.Id = IdGeneratorHelper.Instance.NextId();
                                item.CreateTime = DateTime.Now;
                            }

                            caseInfoList.AddRange(excelData);
                            progressAction.Invoke(new ProcessProgress
                            {
                                BatchId = batchId,
                                Progress = 50,
                                Type = "Filter",
                                Message = $"成功解析Excel文件{Path.GetFileName(excelPath)}，共{excelData.Count}条数据"
                            });                           
                        }
                        catch (Exception ex)
                        {
                            progressAction.Invoke(new ProcessProgress
                            {
                                BatchId = batchId,
                                Progress = 40,
                                Type = "Error",
                                Message = $"解析Excel文件{Path.GetFileName(excelPath)}失败：{ex.Message}"
                            });
                            obj.Status = 0;
                            obj.Message = $"解析Excel失败：{ex.Message}";
                            return obj;
                        }
                    }
                }
                else
                {
                    // 6.2 处理类型1：原有图片OCR识别逻辑
                    progressAction.Invoke(new ProcessProgress
                    {
                        BatchId = batchId,
                        Progress = 30,
                        Type = "Filter",
                        Message = "开始筛选图片文件并执行OCR识别"
                    });

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

                        // 创建Word文档并将图片插入
                        string wordFileName = $"案件相关图片资料_{folderName}.docx";
                        string wordPhysicalPath = Path.Combine(archiveDir, folderName, wordFileName);
                        WordHelper.CreateWordWithImages(imageFiles, wordPhysicalPath);

                        long caseId = IdGeneratorHelper.Instance.NextId();

                        // 识别结果入库
                        caseInfoList.Add(new SysCaseInfoEntity
                        {
                            Id = caseId,
                            CustName = "测试",
                            CustCardNo = "6222021234",
                            CustIDNumber = "1101011234",
                            CreateTime = DateTime.Now
                        });

                        // OCR识别结果字典（图片路径 -> 识别文本）
                        Dictionary<string, string> ocrResults = new Dictionary<string, string>();
                        foreach (var imagePath in imageFiles)
                        {
                            string ocrText = await AIRecognitionHelper.ImageOcrRecognizeAsync(imagePath, batchId, progressAction).ConfigureAwait(false);
                            ocrResults.Add(imagePath, ocrText);                            
                        }
                        ocrResultDict.Add(folderName, ocrResults);
                    }
                }

                // 7. 批量插入数据库（原有逻辑保留）
                if (caseInfoList.Count > 0)
                {
                    await _fileProcessiongDao.InsertAsync(caseInfoList).ConfigureAwait(false);
                    progressAction.Invoke(new ProcessProgress
                    {
                        BatchId = batchId,
                        Progress = 70,
                        Type = "Recognize",
                        Message = $"成功插入{caseInfoList.Count}条案件信息到数据库"
                    });
                }

                // 8. 生成Excel识别结果（兼容两种处理类型）
                string excelFileName = string.Format("处理结果_{0}.xlsx", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                string excelPath_Physical = Path.Combine(exportRootDir_Physical, excelFileName);
                if (processType == 0)
                {
                    // 处理类型0：导出解析后的Excel数据
                    await CompressHelper.GenerateExcelFromEntityAsync(caseInfoList, excelPath_Physical, batchId, progressAction).ConfigureAwait(false);
                }
                else
                {
                    // 处理类型1：原有OCR结果生成Excel
                    await CompressHelper.GenerateOcrExcelAsync(ocrResultDict, excelPath_Physical, batchId, progressAction).ConfigureAwait(false);
                }

                // 9. 复制归档文件+打包+清理（原有逻辑保留）
                foreach (var folderItem in archiveDict)
                {
                    string sourceFolder = Path.Combine(archiveDir, folderItem.Key);
                    string targetFolder = Path.Combine(exportRootDir_Physical, folderItem.Key);
                    CompressHelper.DirectoryCopy(sourceFolder, targetFolder, true);
                }
                await CompressHelper.PackFolderToZipAsync(exportRootDir_Virtual, finalZipPath_Virtual, batchId, progressAction).ConfigureAwait(false);
                await CompressHelper.CleanTempDirAsync(tempRootDir, batchId, progressAction).ConfigureAwait(false);

                obj.Status = 1;
                obj.Message = $"处理完成（处理类型：{(processType == 0 ? "Excel解析" : "图片OCR")}）";
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
                obj.Status = 0;
                obj.Message = $"处理失败：{ex.Message}";
                return obj;
            }
        }
    }
}