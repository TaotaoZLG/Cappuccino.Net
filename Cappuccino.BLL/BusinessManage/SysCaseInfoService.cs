using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Helper;
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
        /// 生成诉状
        /// </summary>
        /// <param name="viewModel">查询条件视图模型</param>
        /// <param name="idsStr">选中行ID字符串（逗号分隔）</param>
        /// <param name="templateId">模板ID</param>
        /// <returns>ZIP文件内存流</returns>
        public async Task<TData> IndictmentAsync(List<SysCaseInfoEntity> caseInfoList, string idsStr, long templateId)
        {
            TData obj = new TData();

            // 2. 获取模板文件路径
            var templateFilePath = _sysTemplateService.GetByIdAsync(templateId).TemplateFilePath;
            if (string.IsNullOrEmpty(templateFilePath))
            {
                obj.Status = 0;
                obj.Message = "模板文件路径为空";
            }
            string templatePhysical = FileHelper.GetPhysicalPath(templateFilePath);
            if (!File.Exists(templatePhysical))
            {
                obj.Status = 0;
                obj.Message = $"模板文件未找到：{templateFilePath}";
            }

            // 3. 生成ZIP内存流
            var zipMemoryStream = new MemoryStream();
            //using (var zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create, true))
            //{
            //    // 4. 遍历案件，替换Word模板并添加到ZIP
            //    foreach (var caseInfo in caseList)
            //    {
            //        // 4.1 读取Word模板并替换域值
            //        var wordStream = ReplaceWordTemplate(templateFilePath, caseInfo);

            //        // 4.2 向ZIP中添加Word文件（命名：案件编号_客户姓名.docx）
            //        string fileName = $"{caseInfo.CaseNo}_{caseInfo.CustName}_诉状.docx";
            //        var zipEntry = zipArchive.CreateEntry(fileName, CompressionLevel.Optimal);
            //        using (var entryStream = zipEntry.Open())
            //        {
            //            wordStream.CopyTo(entryStream);
            //        }
            //        wordStream.Dispose();
            //    }
            //}
            //zipMemoryStream.Position = 0; // 重置流指针，供前端读取
            return obj;
        }
    }
}
