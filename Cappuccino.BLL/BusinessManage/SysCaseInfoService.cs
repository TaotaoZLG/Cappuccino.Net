using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Helpers;
using Cappuccino.Common.Util;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;

namespace Cappuccino.BLL
{
    public class SysCaseInfoService : BaseService<SysCaseInfoEntity>, ISysCaseInfoService
    {
        private ISysCaseInfoDao _sysCaseInfoDao;
        private ISysFileService _sysFileService;
        private ISysTemplateService _sysTemplateService;

        #region 依赖注入
        public SysCaseInfoService(ISysCaseInfoDao sysCaseInfoDao, ISysTemplateService sysTemplateService,ISysFileService sysFileService)
        {
            _sysCaseInfoDao = sysCaseInfoDao;
            _sysTemplateService = sysTemplateService;
            _sysFileService = sysFileService;
            base.CurrentDao = sysCaseInfoDao;
            this.AddDisposableObject(this.CurrentDao);
            this.AddDisposableObject(_sysTemplateService);
        }
        #endregion

        /// <summary>
        /// 批量生成案件Word文档并打包Zip（返回Zip虚拟路径）
        /// 按批次隔离临时文件、自动清理临时文件
        /// </summary>
        /// <param name="caseInfoList">案件列表</param>
        /// <param name="templateId">模板ID</param>
        /// <returns></returns>
        public async Task<TData<string>> IndictmentAsync(List<SysCaseInfoEntity> caseInfoList, long templateId)
        {
            TData<string> obj = new TData<string>();

            // 根据templateId查询模板路径
            var templateEntity = _sysTemplateService.GetTemplateById(templateId);
            string templateFilePath = templateEntity.TemplateFilePath;
            if (templateEntity == null || string.IsNullOrEmpty(templateFilePath))
            {
                obj.Status = 0;
                obj.Message = "模板信息为空或模板路径未配置";
                return obj;
            }
            string templatePhysicalPath = FileHelper.GetPhysicalPath(templateFilePath);
            if (!File.Exists(templatePhysicalPath))
            {
                obj.Status = 0;
                obj.Message = $"模板文件不存在：{templateFilePath}";
                return obj;
            }

            // 批次唯一标识
            string batchId = GuidHelper.GetGuid(true);

            // 构建批次级临时目录（按批次隔离，避免并发冲突）
            string virRootDir = ConfigUtils.AppSetting.GetValue("VirtualDirectory");
            string tempWordVirDir = Path.Combine(virRootDir, "Upload", "TempCaseWord", batchId);
            string tempWordPhysicalDir = FileHelper.GetPhysicalPath(tempWordVirDir);
            FileHelper.CreateDirectory(tempWordPhysicalDir);
          
            // 遍历案件数据，批量生成Word
            try
            {
                int nextWord = 0;
                foreach (var caseInfo in caseInfoList)
                {
                    nextWord++;

                    long caseId = caseInfo.Id;
                    string custName = caseInfo?.CustName;
                    string custIDNumber = caseInfo?.CustIDNumber;

                    string tempWordFileName = $"起诉书_{custName}_{custIDNumber}_{nextWord}.docx";
                    string tempWordPath = Path.Combine(tempWordPhysicalDir, tempWordFileName);

                    // 复制模板文件（覆盖模式）
                    File.Copy(templatePhysicalPath, tempWordPath, true);

                    // 获取案件图片
                    var imageFileList = _sysFileService.GetFilePathById(caseId);

                    // 使用NPOI替换Word域值
                    WordHelper.ReplaceContent(tempWordPath, caseInfo, imageFileList);
                }

                // 压缩为Zip并返回虚拟路径
                string zipFileName = $"案件起诉书_{DateTime.Now:yyyyMMddHHmmss}_{batchId}.zip";
                string zipFilePath = ZipHelper.CompressToZip(tempWordPhysicalDir, tempWordVirDir, zipFileName);

                obj.Status = 1;
                obj.Message = "起诉书生成成功";
                obj.Data = zipFilePath;
                return obj;
            }
            catch (Exception ex)
            {
                obj.Status = 0;
                obj.Message = "起诉书生成失败：" + ex.Message;
            }
            finally
            {
                // 清理临时Word文件
                //FileHelper.DeleteDirectory(tempWordVirDir);
            }
            return obj;
        }

        /// <summary>
        /// 上传文件（支持压缩包上传，解压后返回文件列表）
        /// </summary>
        /// <param name="file"></param>
        /// <param name="saveDirectoryName"></param>
        /// <returns></returns>
        public async Task<TData> UploadFiles(HttpPostedFileBase file, string saveDirectoryName)
        {
            TData obj = new TData();
            try
            {
                if (file == null || file.ContentLength == 0)
                {
                    obj.Status = 0;
                    obj.Message = "请选择要上传的压缩包文件";
                    return obj;
                }

                // 文件格式/大小校验（原有逻辑保留）
                string unzipFileName = file.FileName;
                string supportFormats = ConfigUtils.AppSetting.GetValue("CompressedFileFormats");
                if (!FileHelper.IsValidFileExtension(unzipFileName, supportFormats, '|'))
                {
                    obj.Status = 0;
                    obj.Message = $"当前不支持该文件类型，请尝试其他文件。支持格式：{supportFormats}";
                    return obj;
                }
                int maxSize = ConfigUtils.AppSetting.GetValue("UploadMaxFileSize").ParseToInt();
                if (file.ContentLength > maxSize)
                {
                    obj.Status = 0;
                    obj.Message = $"文件大小超出限制，最大支持{maxSize / 1024 / 1024}MB";
                    return obj;
                }

                string batchId = GuidHelper.GetGuid(true);

                // 上传文件临时路径
                string tempRootPath = ConfigUtils.AppSetting.GetValue("TempRootPath");
                // 压缩包保存路径
                string tempRootPathDir = Path.Combine(tempRootPath, batchId, unzipFileName);
                string tempRootPhysical = FileHelper.GetPhysicalPath(tempRootPathDir);
                FileHelper.EnsureDirectoryExists(tempRootPhysical);
                file.SaveAs(tempRootPhysical);

                // 压缩包解压路径
                string unzipPhysical = Path.Combine(tempRootPhysical, "unzip");
                FileHelper.EnsureDirectoryExists(unzipPhysical);

                // 解压压缩包
                var unzipFiles = CompressHelper.UnzipCompressedFile(tempRootPhysical, unzipPhysical);

                if (unzipFiles.Status == 1)
                {
                    var moveTasks = new List<(string source, string target)>();

                    foreach (var path in unzipFiles.Data)
                    {
                        string fileName = Path.GetFileName(path);
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);
                        string fileExt = Path.GetExtension(path).ToLower();

                        // 查询案件信息（根据文件名匹配案件名称，实际业务可根据需要调整匹配规则）
                        var caseInfoEntity = _sysCaseInfoDao.GetList(x => x.CustName == fileName).FirstOrDefault();
                        if (caseInfoEntity != null)
                        {
                            string archivePathDir = caseInfoEntity.ArchiveVirtualPath;
                            string saveFileDir = Path.Combine(archivePathDir, saveDirectoryName, fileName);
                            string saveFilePhysical = FileHelper.GetPhysicalPath(saveFileDir);
                            FileHelper.CreateDirectory(saveFilePhysical);

                            // 记录移动任务
                            moveTasks.Add((path, saveFilePhysical));
                        }
                    }

                    // 批量移动文件
                    FileHelper.BatchMoveFiles(moveTasks);

                    obj.Status = 1;
                    obj.Message = "文件上传成功";
                }
                else
                {
                    obj.Status = 0;
                    obj.Message = "文件解压失败：" + unzipFiles.Message;
                }
            }
            catch (Exception ex)
            {
                obj.Status = 0;
                obj.Message = "文件上传失败：" + ex.Message;
            }
            return obj;
        }
    }
}
