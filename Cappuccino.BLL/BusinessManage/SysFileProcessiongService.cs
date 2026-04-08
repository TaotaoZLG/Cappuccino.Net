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
using MiniExcelLibs;
using Newtonsoft.Json.Linq;

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

        public async Task<TData<string>> ProcessCompressFileAsync2(HttpPostedFileBase file, int extractRule, int processType, string batchId, Action<ProcessProgress> progressAction)
        {
            TData<string> obj = new TData<string>();
            try
            {
                var userId = UserManager.GetCurrentUserInfo().Id;
                string fileName = file.FileName;                

                // 上传文件临时路径
                string tempRootPath = ConfigUtils.AppSetting.GetValue("TempRootPath");
                string tempRootPhysical = Path.Combine(FileHelper.GetPhysicalPath(tempRootPath), batchId);
                FileHelper.EnsureDirectoryExists(tempRootPhysical);

                string fileVirtualPath = Path.Combine(tempRootPath, batchId, fileName);
                string tempCompressPath = FileHelper.GetPhysicalPath(fileVirtualPath);
                FileHelper.EnsureDirectoryExists(tempCompressPath);
                file.SaveAs(tempCompressPath);                

                // 归档文件路径
                string archiveRootPath = ConfigUtils.AppSetting.GetValue("ArchiveRootPath");
                string archiveRootPathDir = Path.Combine(archiveRootPath, batchId);
                string archivePhysicalRoot = FileHelper.GetPhysicalPath(archiveRootPathDir);
                FileHelper.EnsureDirectoryExists(archiveRootPathDir);

                string unzipPhysical = Path.Combine(tempRootPhysical, "unzip");
                FileHelper.EnsureDirectoryExists(unzipPhysical);

                // 导出文件路径
                string exportRootPath = ConfigUtils.AppSetting.GetValue("ExportRootPath");
                string exportRootDir = Path.Combine(exportRootPath, batchId);
                string exportRootPhysical = Path.Combine(FileHelper.GetPhysicalPath(exportRootPath), batchId);
                FileHelper.EnsureDirectoryExists(exportRootPhysical);

                string finalZipFileName = string.Format("压缩包处理结果_{0}.zip", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                string finalZipPathDir = Path.Combine(exportRootPath, string.Format("final_{0}", finalZipFileName));
                string finalZipPathPhysical = FileHelper.GetPhysicalPath(finalZipPathDir);

                // 解压压缩包
                List<string> unzipFiles = await CompressHelper.UnzipFileAsync(tempCompressPath, unzipPhysical, batchId, progressAction).ConfigureAwait(false);

                // 案件信息
                List<SysCaseInfoEntity> caseInfoList = new List<SysCaseInfoEntity>();
                // OCR记录集合
                List<object> ocrRecordList = new List<object>();

                // 分支处理：根据processType选择Excel解析或OCR识别
                if (processType == 0)
                {
                    // 处理类型0：Excel解析逻辑
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

                    // 解析Excel文件
                    foreach (var excelPath in excelFiles)
                    {
                        try
                        {
                            #region 读取【申请材料】Sheet并去重
                            // 读取申请材料Sheet，提取核心列
                            var applyMaterialList = MiniExcel.Query(
                                path: excelPath,
                                sheetName: "申请材料",
                                useHeaderRow: true)
                                .Select(row => new
                                {
                                    姓名 = row.姓名?.ToString()?.Trim(),
                                    卡号 = row.卡号?.ToString()?.Trim(),
                                    案件号 = row.案件号?.ToString()?.Trim()
                                })
                                .Where(x => !string.IsNullOrEmpty(x.姓名) && !string.IsNullOrEmpty(x.卡号)) // 过滤空数据
                                .GroupBy(x => new { x.姓名, x.卡号 }) // 按姓名+卡号去重
                                .Select(g => g.First()) // 去重后取第一条
                                .ToList();

                            #endregion

                            #region 读取【申请编号】Sheet并关联合并（插入案件号）
                            // 读取申请编号Sheet所有列
                            var applyNoList = MiniExcel.Query(
                                path: excelPath,
                                sheetName: "申请编号",
                                useHeaderRow: true)
                                .Select(row => new
                                {
                                    姓名 = row.姓名?.ToString()?.Trim(),
                                    卡号 = row.当前卡号?.ToString()?.Trim(),
                                    // 保留所有列数据（键值对）
                                    AllColumns = ((IDictionary<string, object>)row).ToDictionary(
                                        k => k.Key,
                                        v => v.Value?.ToString()?.Trim()
                                    )
                                })
                                .ToList();

                            // 关联申请材料和申请编号，插入案件号
                            var applyNoMergeList = applyMaterialList
                                .Join(applyNoList,
                                    material => new { material.姓名, material.卡号 },
                                    no => new { no.姓名, no.卡号 },
                                    (material, no) =>
                                    {
                                        var mergeDict = new Dictionary<string, string>(no.AllColumns);
                                        mergeDict["案件号"] = material.案件号; // 插入案件号列
                                        mergeDict["姓名"] = no.姓名; // 存入姓名
                                        mergeDict["卡号"] = no.卡号; // 存入当前卡号
                                        return mergeDict;
                                    })
                                .ToList();

                            #endregion

                            #region 读取【金额拆分】Sheet并【仅插入指定4列到最前面】
                            // 读取金额拆分Sheet所有列
                            var amountSplitList = MiniExcel.Query(
                                path: excelPath,
                                sheetName: "金额拆分",
                                useHeaderRow: true)
                                .Select(row => new
                                {
                                    姓名 = row.姓名?.ToString()?.Trim(),
                                    卡号 = row.卡号?.ToString()?.Trim(),
                                    // 保留所有列数据
                                    AllColumns = ((IDictionary<string, object>)row).ToDictionary(
                                        k => k.Key,
                                        v => v.Value?.ToString()?.Trim()
                                    )
                                })
                                .Where(x => !string.IsNullOrEmpty(x.姓名) && !string.IsNullOrEmpty(x.卡号))
                                .ToList();

                            //  【核心修改：仅插入4个指定列 + 置顶】
                            var finalMergeList = new List<Dictionary<string, string>>();
                            // 定义需要插入的固定列（仅这4个）
                            var targetColumns = new[] { "证件号码", "申请编号", "来源代码", "案件号" };

                            foreach (var split in amountSplitList)
                            {
                                var newRow = new Dictionary<string, string>();

                                // 1. 匹配数据，仅插入指定的4列（放在最前面）
                                var matchData = applyNoMergeList.FirstOrDefault(m =>
                                    m["姓名"] == split.姓名 &&
                                    m["卡号"] == split.卡号
                                );
                                if (matchData != null)
                                {
                                    foreach (var col in targetColumns)
                                    {
                                        // 存在该列则插入，不存在则留空
                                        newRow[col] = matchData.TryGetValue(col, out string val) ? val : "";
                                    }
                                }

                                // 2. 追加金额拆分原始所有数据（跟在指定列后面）
                                foreach (var kv in split.AllColumns)
                                {
                                    if (!newRow.ContainsKey(kv.Key))
                                    {
                                        newRow[kv.Key] = kv.Value;
                                    }
                                }

                                finalMergeList.Add(newRow);
                            }
                            #endregion

                            #region 合并数据保存为 Excel 到服务器本地
                            // 配置保存目录（自动创建，不存在则新建）
                            string saveRootPath = FileHelper.GetPhysicalPath(archiveRootPathDir);
                            FileHelper.EnsureDirectoryExists(saveRootPath);

                            // 生成唯一文件名（避免重复）
                            var saveFileName = $"Excel多表合并结果_{batchId}.xlsx";
                            var saveFullPath = Path.Combine(archivePhysicalRoot, saveFileName); // 本地物理路径
                            var saveRelativePath = $"{archiveRootPathDir}/{saveFileName}"; // 网站相对访问路径

                            // 保存合并数据到Excel
                            MiniExcelHelper.ExportToExcel(finalMergeList, saveFullPath);
                            //MiniExcel.SaveAs(saveFullPath, finalMergeList);
                            // 复制到导出目录
                            File.Copy(saveFullPath, Path.Combine(exportRootPhysical, saveFileName));
                            #endregion

                            #region 文件自动归档（匹配3个目录 + 统一归档）
                            // 基础配置
                            string archiveVirtualRoot = archiveRootPathDir;
                            char[] invalidChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray();

                            // 获取解压后的3个目标目录
                            string unzipVirtualPath = Path.Combine(unzipPhysical, Path.GetFileNameWithoutExtension(fileName));
                            string imgAmountDir = Directory.GetDirectories(unzipVirtualPath, "影像金额", SearchOption.AllDirectories).FirstOrDefault();
                            string sealDir = Directory.GetDirectories(unzipVirtualPath, "电子章", SearchOption.AllDirectories).FirstOrDefault();
                            string addressDir = Directory.GetDirectories(unzipVirtualPath, "地址截屏", SearchOption.AllDirectories).FirstOrDefault();

                            // 遍历最终合并数据，逐个归档
                            foreach (var data in finalMergeList)
                            {
                                // 读取关键字段（增加空值保护）
                                data.TryGetValue("姓名", out string userName);
                                data.TryGetValue("卡号", out string cardNo);
                                data.TryGetValue("证件号码", out string idNumber);
                                data.TryGetValue("案件号", out string caseNo);
                                data.TryGetValue("申请编号", out string applyNo);
                                data.TryGetValue("人民币消费本金余额", out string principalAmount);
                                data.TryGetValue("人民币消费利息余额", out string interestAmount);

                                var (Gender, BirthDate) = OtherHelper.ParseIdCard(idNumber);
                                string last4Card = cardNo.Length >= 4 ? cardNo.Substring(cardNo.Length - 4) : cardNo;
                                // 存储OCR识别结果
                                string ocrResult = string.Empty;

                                // 生成归档文件夹：姓名+卡号（过滤非法字符）
                                string folderName = new string($"{userName}{cardNo}".Where(c => !invalidChars.Contains(c)).ToArray());
                                string targetPhysicalPath = Path.Combine(archivePhysicalRoot, folderName);  //归档存储物理路径
                                string targetVirtualPath = $"{archiveVirtualRoot}/{folderName}/";  //归档存储虚拟路径
                                Directory.CreateDirectory(targetPhysicalPath);

                                // 创建分类子文件夹
                                string targetImageDir = Path.Combine(targetPhysicalPath, "影像图片");
                                Directory.CreateDirectory(targetImageDir);

                                // 地址截屏集合（后续OCR识别）初始化
                                IEnumerable<string> targetScreenshotFiles = Enumerable.Empty<string>();

                                // 过滤图片集合
                                List<string> imageFiles = new List<string>();
                                // 图片集合，用于后续创建Word文档（合并所有来源的图片）
                                List<string> allImagesList = new List<string>();

                                // ============== 1. 影像金额 → 匹配【最后一级】子文件夹名 ==============
                                if (!string.IsNullOrEmpty(imgAmountDir) && Directory.Exists(imgAmountDir))
                                {
                                    IEnumerable<string> targetImageFiles = Enumerable.Empty<string>();

                                    // 步骤1：获取影像金额下【所有层级】的子文件夹
                                    var allSubFolders = Directory.GetDirectories(imgAmountDir, "*", SearchOption.AllDirectories);
                                    // 步骤2：筛选出【最后一级】子文件夹（没有子文件夹的就是最底层）
                                    var lastLevelFolders = allSubFolders.Where(f => !Directory.EnumerateDirectories(f).Any());
                                    // 步骤3：筛选最后一级文件夹名包含申请编号的
                                    var matchFolders = lastLevelFolders.Where(f => Path.GetFileName(f).Contains(applyNo));

                                    if (matchFolders.Any())
                                    {
                                        // 遍历匹配的最后一级文件夹内的所有文件
                                        targetImageFiles = matchFolders.SelectMany(f => Directory.GetFiles(f, "*.*", SearchOption.AllDirectories));
                                    }
                                    else
                                    {
                                        // 降级逻辑：无匹配最后一级文件夹时，按原有规则匹配文件名
                                        targetImageFiles = Directory.GetFiles(imgAmountDir, "*.*", SearchOption.AllDirectories).Where(f => Path.GetFileName(f).Contains(applyNo));
                                    }

                                    //CompressHelper.CopyFiles(targetImageFiles, targetPhysicalPath);

                                    // 复制到【影像图片】分类文件夹
                                    CompressHelper.CopyFiles(targetImageFiles, targetImageDir);

                                    // 过滤图片
                                    imageFiles = await CompressHelper.FilterImageFilesAsync(targetImageFiles.ToList(), batchId, progressAction).ConfigureAwait(false);
                                    allImagesList.AddRange(imageFiles);
                                }

                                // ============== 2. 电子章 → 先匹配子文件夹名，再取文件 ==============
                                if (!string.IsNullOrEmpty(sealDir) && Directory.Exists(sealDir))
                                {
                                    IEnumerable<string> targetSealFiles = Enumerable.Empty<string>();

                                    // 步骤1：获取电子章下的所有直接子文件夹
                                    var subFolders = Directory.GetDirectories(sealDir, "*", SearchOption.TopDirectoryOnly);
                                    // 步骤2：筛选子文件夹名包含案件号的（核心修改）
                                    var matchSubFolders = subFolders.Where(f => Path.GetFileName(f).Contains(caseNo));

                                    if (matchSubFolders.Any())
                                    {
                                        // 若找到匹配的子文件夹，仅遍历这些文件夹内的文件
                                        targetSealFiles = matchSubFolders.SelectMany(f => Directory.GetFiles(f, "*.*", SearchOption.AllDirectories));
                                    }
                                    else
                                    {
                                        // 降级逻辑：无匹配子文件夹时，按原有规则匹配文件名
                                        targetSealFiles = Directory.GetFiles(sealDir, "*.*", SearchOption.AllDirectories).Where(f => Path.GetFileName(f).Contains(caseNo));
                                    }

                                    CompressHelper.CopyFiles(targetSealFiles, targetPhysicalPath);

                                    // 过滤图片
                                    imageFiles = await CompressHelper.FilterImageFilesAsync(targetSealFiles.ToList(), batchId, progressAction).ConfigureAwait(false);
                                    allImagesList.AddRange(imageFiles);
                                }

                                // ============== 3. 地址截屏（若有嵌套可参考上面修改）+ OCR识别 ==============
                                if (!string.IsNullOrEmpty(addressDir) && Directory.Exists(addressDir))
                                {
                                    string matchKey = $"{userName}{last4Card}";
                                    targetScreenshotFiles = Directory.GetFiles(addressDir, "*.*", SearchOption.AllDirectories).Where(f => Path.GetFileName(f).Contains(matchKey));
                                    CompressHelper.CopyFiles(targetScreenshotFiles, targetPhysicalPath);
                                }

                                // 创建Word文档并将图片插入
                                if (allImagesList.Any())
                                {
                                    string wordPath = Path.Combine(targetPhysicalPath, $"{userName}{cardNo}_图片资料归档.docx");
                                    NpoiHelper.CreateWordWithImages(allImagesList, wordPath);
                                }

                                // OCR识别获取地址信息（地址截屏目录下Word 仅第一张图片）
                                if (targetScreenshotFiles.Any())
                                {
                                    string[] WordExtensions = { ".docx", ".doc" };
                                    var addressWordFiles = targetScreenshotFiles.Where(f => WordExtensions.Contains(Path.GetExtension(f).ToLower())).ToList();

                                    if (addressWordFiles.Any())
                                    {
                                        string firstWordPath = addressWordFiles.FirstOrDefault();

                                        if (!string.IsNullOrEmpty(firstWordPath))
                                        {
                                            // 提取Word中的第一张图片
                                            (string result, string tempImgPath, string wordFileName, string imageFileName) = NpoiHelper.ExtractFirstImageFromWord(firstWordPath, exportRootPhysical);

                                            // OCR识别
                                            //ocrResult = await AIRecognitionHelper.ImageOcrRecognizeAsync(tempImgPath, batchId, progressAction).ConfigureAwait(false);

                                            ocrResult = "{\"detail\": \"不支持的图片格式：docx\"}";
                                            var objData = JsonHelper.ToJObject(ocrResult);
                                            string residentialAddress = objData.Value<string>("住宅地址");
                                            string unitAddress = objData.Value<string>("单位地址");
                                            string mailingAddress = objData.Value<string>("邮寄地址");
                                            string phone = objData.Value<string>("手机");

                                            ocrRecordList.Add(new
                                            {
                                                图片所在Word文件名 = wordFileName,
                                                图片文件名 = imageFileName,
                                                识别结果 = ocrResult,
                                                识别时间 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                            });

                                            // 组装入库数据
                                            caseInfoList.Add(new SysCaseInfoEntity
                                            {
                                                Id = IdGeneratorHelper.Instance.NextId(),
                                                CustName = userName,
                                                CustCardNo = cardNo,
                                                CustIDNumber = idNumber,
                                                CaseNumber = caseNo,
                                                CustBirthdate = BirthDate.ParseToDateTime(),
                                                CustGender = Gender,
                                                ApplayNumber = applyNo,
                                                PrincipalAmount = principalAmount.ParseToDecimal(),
                                                InterestAmount = interestAmount.ParseToDecimal(),
                                                ContactPhone = phone,
                                                MailingAddress = mailingAddress,
                                                CompanyAddress = unitAddress,
                                                HomeAddress = residentialAddress,
                                                CreateTime = DateTime.Now,
                                                CreateUserId = userId,
                                                ArchiveVirtualPath = targetVirtualPath
                                            });
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region 批量生成OCR记录Excel
                            if (ocrRecordList.Any())
                            {
                                MiniExcelHelper.ExportToExcel(ocrRecordList, exportRootPhysical, $"OCR识别记录_{batchId}.xlsx");
                            }
                            #endregion

                            #region 批量插入数据库
                            // 批量插入数据库
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
                            #endregion

                            #region 打包导出（合并excel + OCR识别结果）
                            await CompressHelper.PackFolderToZipAsync(exportRootDir, finalZipPathDir, batchId, progressAction).ConfigureAwait(false);
                            #endregion
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
                        finally
                        {
                            //try
                            //{
                            //    // 清理解压目录
                            //    await CompressHelper.CleanTempDirAsync(unzipPhysical, batchId, progressAction).ConfigureAwait(false);
                            //    // 清理临时目录
                            //    await CompressHelper.CleanTempDirAsync(tempRootPhysical, batchId, progressAction).ConfigureAwait(false);
                            //    // 清理导出目录
                            //    await CompressHelper.CleanTempDirAsync(exportRootPhysical, batchId, progressAction).ConfigureAwait(false);
                            //    // 清理最终Zip文件
                            //    await CompressHelper.CleanTempDirAsync(finalZipPathPhysical, batchId, progressAction).ConfigureAwait(false);
                            //}
                            //catch (Exception ex)
                            //{
                            //    progressAction.Invoke(new ProcessProgress
                            //    {
                            //        BatchId = batchId,
                            //        Progress = 0,
                            //        Type = "Error",
                            //        Message = $"清理临时解压文件失败：{ex.Message}"
                            //    });
                            //}
                        }
                    }
                }

                obj.Status = 1;
                obj.Message = $"处理完成（处理类型：{(processType == 0 ? "Excel解析" : "图片OCR")}），请在浏览器文件下载记录中查看处理结果文件";
                obj.Data = finalZipPathDir;
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
            }
            return obj;
        }

        public async Task<TData<string>> ProcessCompressFileAsync(HttpPostedFileBase file, int extractRule, int processType, string batchId, Action<ProcessProgress> progressAction)
        {
            TData<string> obj = new TData<string>();
            try
            {
                var userId = UserManager.GetCurrentUserInfo().Id;
                string fileName = file.FileName;

                // 上传文件临时路径
                string tempRootPath = ConfigUtils.AppSetting.GetValue("TempRootPath");
                string tempRootPhysical = Path.Combine(FileHelper.GetPhysicalPath(tempRootPath), batchId);
                FileHelper.EnsureDirectoryExists(tempRootPhysical);

                string fileVirtualPath = Path.Combine(tempRootPath, batchId, fileName);
                string tempCompressPath = FileHelper.GetPhysicalPath(fileVirtualPath);
                FileHelper.EnsureDirectoryExists(tempCompressPath);
                file.SaveAs(tempCompressPath);

                // 归档文件路径
                string archiveRootPath = ConfigUtils.AppSetting.GetValue("ArchiveRootPath");
                string archiveRootPathDir = Path.Combine(archiveRootPath, batchId);
                string archivePhysicalRoot = FileHelper.GetPhysicalPath(archiveRootPathDir);
                FileHelper.EnsureDirectoryExists(archiveRootPathDir);

                string unzipPhysical = Path.Combine(tempRootPhysical, "unzip");
                FileHelper.EnsureDirectoryExists(unzipPhysical);

                // 导出文件路径
                string exportRootPath = ConfigUtils.AppSetting.GetValue("ExportRootPath");
                string exportRootDir = Path.Combine(exportRootPath, batchId);
                string exportRootPhysical = Path.Combine(FileHelper.GetPhysicalPath(exportRootPath), batchId);
                FileHelper.EnsureDirectoryExists(exportRootPhysical);

                string finalZipFileName = string.Format("压缩包处理结果_{0}.zip", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                string finalZipPathDir = Path.Combine(exportRootPath, string.Format("final_{0}", finalZipFileName));
                string finalZipPathPhysical = FileHelper.GetPhysicalPath(finalZipPathDir);

                // 解压压缩包
                List<string> unzipFiles = await CompressHelper.UnzipFileAsync(tempCompressPath, unzipPhysical, batchId, progressAction).ConfigureAwait(false);

                // 案件信息
                List<SysCaseInfoEntity> caseInfoList = new List<SysCaseInfoEntity>();
                // OCR记录集合
                List<object> ocrRecordList = new List<object>();

                // 分支处理：根据processType选择Excel解析或OCR识别
                if (processType == 0)
                {
                    // 处理类型0：Excel解析逻辑
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

                    // 解析Excel文件
                    foreach (var excelPath in excelFiles)
                    {
                        try
                        {
                            #region 读取【申请材料】Sheet并去重
                            // 读取申请材料Sheet，提取核心列
                            var applyMaterialList = MiniExcel.Query(
                                path: excelPath,
                                sheetName: "申请材料",
                                useHeaderRow: true)
                                .Select(row => new
                                {
                                    姓名 = row.姓名?.ToString()?.Trim(),
                                    卡号 = row.卡号?.ToString()?.Trim(),
                                    案件号 = row.案件号?.ToString()?.Trim()
                                })
                                .Where(x => !string.IsNullOrEmpty(x.姓名) && !string.IsNullOrEmpty(x.卡号)) // 过滤空数据
                                .GroupBy(x => new { x.姓名, x.卡号 }) // 按姓名+卡号去重
                                .Select(g => g.First()) // 去重后取第一条
                                .ToList();

                            #endregion

                            #region 读取【申请编号】Sheet并关联合并（插入案件号）
                            // 读取申请编号Sheet所有列
                            var applyNoList = MiniExcel.Query(
                                path: excelPath,
                                sheetName: "申请编号",
                                useHeaderRow: true)
                                .Select(row => new
                                {
                                    姓名 = row.姓名?.ToString()?.Trim(),
                                    卡号 = row.当前卡号?.ToString()?.Trim(),
                                    // 保留所有列数据（键值对）
                                    AllColumns = ((IDictionary<string, object>)row).ToDictionary(
                                        k => k.Key,
                                        v => v.Value?.ToString()?.Trim()
                                    )
                                })
                                .ToList();

                            // 关联申请材料和申请编号，插入案件号
                            var applyNoMergeList = applyMaterialList
                                .Join(applyNoList,
                                    material => new { material.姓名, material.卡号 },
                                    no => new { no.姓名, no.卡号 },
                                    (material, no) =>
                                    {
                                        var mergeDict = new Dictionary<string, string>(no.AllColumns);
                                        mergeDict["案件号"] = material.案件号; // 插入案件号列
                                        mergeDict["姓名"] = no.姓名; // 存入姓名
                                        mergeDict["卡号"] = no.卡号; // 存入当前卡号
                                        return mergeDict;
                                    })
                                .ToList();

                            #endregion

                            #region 读取【金额拆分】Sheet并【仅插入指定4列到最前面】
                            // 读取金额拆分Sheet所有列
                            var amountSplitList = MiniExcel.Query(
                                path: excelPath,
                                sheetName: "金额拆分",
                                useHeaderRow: true)
                                .Select(row => new
                                {
                                    姓名 = row.姓名?.ToString()?.Trim(),
                                    卡号 = row.卡号?.ToString()?.Trim(),
                                    // 保留所有列数据
                                    AllColumns = ((IDictionary<string, object>)row).ToDictionary(
                                        k => k.Key,
                                        v => v.Value?.ToString()?.Trim()
                                    )
                                })
                                .Where(x => !string.IsNullOrEmpty(x.姓名) && !string.IsNullOrEmpty(x.卡号))
                                .ToList();

                            //  【核心修改：仅插入4个指定列 + 置顶】
                            var finalMergeList = new List<Dictionary<string, string>>();
                            // 定义需要插入的固定列（仅这4个）
                            var targetColumns = new[] { "证件号码", "申请编号", "来源代码", "案件号" };

                            foreach (var split in amountSplitList)
                            {
                                var newRow = new Dictionary<string, string>();

                                // 1. 匹配数据，仅插入指定的4列（放在最前面）
                                var matchData = applyNoMergeList.FirstOrDefault(m =>
                                    m["姓名"] == split.姓名 &&
                                    m["卡号"] == split.卡号
                                );
                                if (matchData != null)
                                {
                                    foreach (var col in targetColumns)
                                    {
                                        // 存在该列则插入，不存在则留空
                                        newRow[col] = matchData.TryGetValue(col, out string val) ? val : "";
                                    }
                                }

                                // 2. 追加金额拆分原始所有数据（跟在指定列后面）
                                foreach (var kv in split.AllColumns)
                                {
                                    if (!newRow.ContainsKey(kv.Key))
                                    {
                                        newRow[kv.Key] = kv.Value;
                                    }
                                }

                                finalMergeList.Add(newRow);
                            }
                            #endregion

                            #region 合并数据保存为 Excel 到服务器本地
                            // 配置保存目录（自动创建，不存在则新建）
                            string saveRootPath = FileHelper.GetPhysicalPath(archiveRootPathDir);
                            FileHelper.EnsureDirectoryExists(saveRootPath);

                            // 生成唯一文件名（避免重复）
                            var saveFileName = $"Excel多表合并结果_{batchId}.xlsx";
                            var saveFullPath = Path.Combine(archivePhysicalRoot, saveFileName); // 本地物理路径
                            var saveRelativePath = $"{archiveRootPathDir}/{saveFileName}"; // 网站相对访问路径

                            // 保存合并数据到Excel
                            MiniExcelHelper.ExportToExcel(finalMergeList, saveFullPath);
                            //MiniExcel.SaveAs(saveFullPath, finalMergeList);
                            // 复制到导出目录
                            File.Copy(saveFullPath, Path.Combine(exportRootPhysical, saveFileName));
                            #endregion

                            #region 文件自动归档（匹配3个目录 + 统一归档）
                            // 基础配置
                            string archiveVirtualRoot = archiveRootPathDir;
                            char[] invalidChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray();

                            // 获取解压后的3个目标目录
                            string unzipVirtualPath = Path.Combine(unzipPhysical, Path.GetFileNameWithoutExtension(fileName));
                            string imgAmountDir = Directory.GetDirectories(unzipVirtualPath, "影像金额", SearchOption.AllDirectories).FirstOrDefault();
                            string sealDir = Directory.GetDirectories(unzipVirtualPath, "电子章", SearchOption.AllDirectories).FirstOrDefault();
                            string addressDir = Directory.GetDirectories(unzipVirtualPath, "地址截屏", SearchOption.AllDirectories).FirstOrDefault();

                            // 遍历最终合并数据，逐个归档
                            foreach (var data in finalMergeList)
                            {
                                // 读取关键字段（增加空值保护）
                                data.TryGetValue("姓名", out string userName);
                                data.TryGetValue("卡号", out string cardNo);
                                data.TryGetValue("证件号码", out string idNumber);
                                data.TryGetValue("案件号", out string caseNo);
                                data.TryGetValue("申请编号", out string applyNo);
                                data.TryGetValue("人民币消费本金余额", out string principalAmount);
                                data.TryGetValue("人民币消费利息余额", out string interestAmount);

                                var (Gender, BirthDate) = OtherHelper.ParseIdCard(idNumber);
                                string last4Card = cardNo.Length >= 4 ? cardNo.Substring(cardNo.Length - 4) : cardNo;
                                // 存储OCR识别结果
                                string ocrResult = string.Empty;

                                // 生成归档文件夹：姓名+卡号（过滤非法字符）
                                string folderName = new string($"{userName}{cardNo}".Where(c => !invalidChars.Contains(c)).ToArray());
                                string targetPhysicalPath = Path.Combine(archivePhysicalRoot, folderName);  //归档存储物理路径
                                string targetVirtualPath = $"{archiveVirtualRoot}/{folderName}/";  //归档存储虚拟路径
                                Directory.CreateDirectory(targetPhysicalPath);

                                // 创建分类子文件夹
                                string targetImageDir = Path.Combine(targetPhysicalPath, "影像图片");
                                Directory.CreateDirectory(targetImageDir);

                                // 地址截屏集合（后续OCR识别）初始化
                                IEnumerable<string> targetScreenshotFiles = Enumerable.Empty<string>();

                                // 过滤图片集合
                                List<string> imageFiles = new List<string>();
                                // 图片集合，用于后续创建Word文档（合并所有来源的图片）
                                List<string> allImagesList = new List<string>();

                                // ============== 1. 影像金额 → 匹配【最后一级】子文件夹名 ==============
                                if (!string.IsNullOrEmpty(imgAmountDir) && Directory.Exists(imgAmountDir))
                                {
                                    IEnumerable<string> targetImageFiles = Enumerable.Empty<string>();

                                    // 步骤1：获取影像金额下【所有层级】的子文件夹
                                    var allSubFolders = Directory.GetDirectories(imgAmountDir, "*", SearchOption.AllDirectories);
                                    // 步骤2：筛选出【最后一级】子文件夹（没有子文件夹的就是最底层）
                                    var lastLevelFolders = allSubFolders.Where(f => !Directory.EnumerateDirectories(f).Any());
                                    // 步骤3：筛选最后一级文件夹名包含申请编号的
                                    var matchFolders = lastLevelFolders.Where(f => Path.GetFileName(f).Contains(applyNo));

                                    if (matchFolders.Any())
                                    {
                                        // 遍历匹配的最后一级文件夹内的所有文件
                                        targetImageFiles = matchFolders.SelectMany(f => Directory.GetFiles(f, "*.*", SearchOption.AllDirectories));
                                    }
                                    else
                                    {
                                        // 降级逻辑：无匹配最后一级文件夹时，按原有规则匹配文件名
                                        targetImageFiles = Directory.GetFiles(imgAmountDir, "*.*", SearchOption.AllDirectories).Where(f => Path.GetFileName(f).Contains(applyNo));
                                    }

                                    //CompressHelper.CopyFiles(targetImageFiles, targetPhysicalPath);

                                    // 复制到【影像图片】分类子文件夹
                                    CompressHelper.CopyFiles(targetImageFiles, targetImageDir);

                                    // 过滤图片
                                    imageFiles = await CompressHelper.FilterImageFilesAsync(targetImageFiles.ToList(), batchId, progressAction).ConfigureAwait(false);
                                    allImagesList.AddRange(imageFiles);
                                }

                                // ============== 2. 电子章 → 先匹配子文件夹名，再取文件 ==============
                                if (!string.IsNullOrEmpty(sealDir) && Directory.Exists(sealDir))
                                {
                                    IEnumerable<string> targetSealFiles = Enumerable.Empty<string>();

                                    // 步骤1：获取电子章下的所有直接子文件夹
                                    var subFolders = Directory.GetDirectories(sealDir, "*", SearchOption.TopDirectoryOnly);
                                    // 步骤2：筛选子文件夹名包含案件号的（核心修改）
                                    var matchSubFolders = subFolders.Where(f => Path.GetFileName(f).Contains(caseNo));

                                    if (matchSubFolders.Any())
                                    {
                                        // 若找到匹配的子文件夹，仅遍历这些文件夹内的文件
                                        targetSealFiles = matchSubFolders.SelectMany(f => Directory.GetFiles(f, "*.*", SearchOption.AllDirectories));
                                    }
                                    else
                                    {
                                        // 降级逻辑：无匹配子文件夹时，按原有规则匹配文件名
                                        targetSealFiles = Directory.GetFiles(sealDir, "*.*", SearchOption.AllDirectories).Where(f => Path.GetFileName(f).Contains(caseNo));
                                    }

                                    CompressHelper.CopyFiles(targetSealFiles, targetPhysicalPath);

                                    // 过滤图片
                                    imageFiles = await CompressHelper.FilterImageFilesAsync(targetSealFiles.ToList(), batchId, progressAction).ConfigureAwait(false);
                                    allImagesList.AddRange(imageFiles);
                                }

                                // ============== 3. 地址截屏（若有嵌套可参考上面修改）+ OCR识别 ==============
                                if (!string.IsNullOrEmpty(addressDir) && Directory.Exists(addressDir))
                                {
                                    string matchKey = $"{userName}{last4Card}";
                                    targetScreenshotFiles = Directory.GetFiles(addressDir, "*.*", SearchOption.AllDirectories).Where(f => Path.GetFileName(f).Contains(matchKey));
                                    CompressHelper.CopyFiles(targetScreenshotFiles, targetPhysicalPath);
                                }

                                // 创建Word文档并将图片插入
                                if (allImagesList.Any())
                                {
                                    string wordPath = Path.Combine(targetPhysicalPath, $"{userName}{cardNo}_图片资料归档.docx");
                                    NpoiHelper.CreateWordWithImages(allImagesList, wordPath);
                                }

                                // OCR识别获取地址信息（地址截屏目录下Word 仅第一张图片）
                                if (targetScreenshotFiles.Any())
                                {
                                    string[] WordExtensions = { ".docx", ".doc" };
                                    var addressWordFiles = targetScreenshotFiles.Where(f => WordExtensions.Contains(Path.GetExtension(f).ToLower())).ToList();

                                    if (addressWordFiles.Any())
                                    {
                                        string firstWordPath = addressWordFiles.FirstOrDefault();

                                        if (!string.IsNullOrEmpty(firstWordPath))
                                        {
                                            // 提取Word中的第一张图片
                                            (string result, string tempImgPath, string wordFileName, string imageFileName) = NpoiHelper.ExtractFirstImageFromWord(firstWordPath, exportRootPhysical);

                                            // OCR识别
                                            //ocrResult = await AIRecognitionHelper.ImageOcrRecognizeAsync(tempImgPath, batchId, progressAction).ConfigureAwait(false);

                                            ocrResult = "{\"detail\": \"不支持的图片格式：docx\"}";
                                            var objData = JsonHelper.ToJObject(ocrResult);
                                            string residentialAddress = objData.Value<string>("住宅地址");
                                            string unitAddress = objData.Value<string>("单位地址");
                                            string mailingAddress = objData.Value<string>("邮寄地址");
                                            string phone = objData.Value<string>("手机");

                                            ocrRecordList.Add(new
                                            {
                                                图片所在Word文件名 = wordFileName,
                                                图片文件名 = imageFileName,
                                                识别结果 = ocrResult,
                                                识别时间 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                            });

                                            // 组装入库数据
                                            caseInfoList.Add(new SysCaseInfoEntity
                                            {
                                                Id = IdGeneratorHelper.Instance.NextId(),
                                                CustName = userName,
                                                CustCardNo = cardNo,
                                                CustIDNumber = idNumber,
                                                CaseNumber = caseNo,
                                                CustBirthdate = BirthDate.ParseToDateTime(),
                                                CustGender = Gender,
                                                ApplayNumber = applyNo,
                                                PrincipalAmount = principalAmount.ParseToDecimal(),
                                                InterestAmount = interestAmount.ParseToDecimal(),
                                                ContactPhone = phone,
                                                MailingAddress = mailingAddress,
                                                CompanyAddress = unitAddress,
                                                HomeAddress = residentialAddress,
                                                CreateTime = DateTime.Now,
                                                CreateUserId = userId,
                                                ArchiveVirtualPath = targetVirtualPath
                                            });
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region 批量生成OCR记录Excel
                            if (ocrRecordList.Any())
                            {
                                MiniExcelHelper.ExportToExcel(ocrRecordList, exportRootPhysical, $"OCR识别记录_{batchId}.xlsx");
                            }
                            #endregion

                            #region 批量插入数据库
                            // 批量插入数据库
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
                            #endregion

                            #region 打包导出（合并excel + OCR识别结果）
                            await CompressHelper.PackFolderToZipAsync(exportRootDir, finalZipPathDir, batchId, progressAction).ConfigureAwait(false);
                            #endregion
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
                        finally
                        {
                            //try
                            //{
                            //    // 清理解压目录
                            //    await CompressHelper.CleanTempDirAsync(unzipPhysical, batchId, progressAction).ConfigureAwait(false);
                            //    // 清理临时目录
                            //    await CompressHelper.CleanTempDirAsync(tempRootPhysical, batchId, progressAction).ConfigureAwait(false);
                            //    // 清理导出目录
                            //    await CompressHelper.CleanTempDirAsync(exportRootPhysical, batchId, progressAction).ConfigureAwait(false);
                            //    // 清理最终Zip文件
                            //    await CompressHelper.CleanTempDirAsync(finalZipPathPhysical, batchId, progressAction).ConfigureAwait(false);
                            //}
                            //catch (Exception ex)
                            //{
                            //    progressAction.Invoke(new ProcessProgress
                            //    {
                            //        BatchId = batchId,
                            //        Progress = 0,
                            //        Type = "Error",
                            //        Message = $"清理临时解压文件失败：{ex.Message}"
                            //    });
                            //}
                        }
                    }
                }

                obj.Status = 1;
                obj.Message = $"处理完成（处理类型：{(processType == 0 ? "Excel解析" : "图片OCR")}），请在浏览器文件下载记录中查看处理结果文件";
                obj.Data = finalZipPathDir;
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
            }
            return obj;
        }
    }
}