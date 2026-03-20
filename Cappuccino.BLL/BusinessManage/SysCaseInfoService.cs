using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        private ISysTemplateService _sysTemplateService;

        #region 依赖注入
        public SysCaseInfoService(ISysCaseInfoDao sysCaseInfoDao, ISysTemplateService sysTemplateService)
        {
            _sysCaseInfoDao = sysCaseInfoDao;
            _sysTemplateService = sysTemplateService;
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
        public async Task<TData> IndictmentAsync(List<SysCaseInfoEntity> caseInfoList, long templateId)
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

                    // 复制模板到临时文件（避免修改原模板）
                    string custName = caseInfo?.CustName;
                    string custIDNumber = caseInfo?.CustIDNumber;

                    string guid = IdGeneratorHelper.Instance.NextId().ToString();
                    string tempWordFileName = $"起诉书_{custName}_{custIDNumber}_{nextWord}.docx";
                    string tempWordPath = Path.Combine(tempWordPhysicalDir, tempWordFileName);

                    // 复制模板文件（覆盖模式）
                    File.Copy(templatePhysicalPath, tempWordPath, true);

                    // 使用NPOI替换Word域值
                    WordHelper.ReplaceContent(tempWordPath, caseInfo);
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
    }
}
