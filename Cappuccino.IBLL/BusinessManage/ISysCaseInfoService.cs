using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Cappuccino.Common.Util;
using Cappuccino.Entity;
using Cappuccino.Model;

namespace Cappuccino.IBLL
{
    public interface ISysCaseInfoService : IBaseService<SysCaseInfoEntity>
    {
        /// <summary>
        /// 批量生成案件Word文档并打包Zip（返回Zip虚拟路径）
        /// 按批次隔离临时文件、自动清理临时文件
        /// </summary>
        /// <param name="caseInfoList">案件列表</param>
        /// <param name="templateId">模板ID</param>
        /// <returns></returns>
        Task<TData<string>> IndictmentAsync(List<SysCaseInfoEntity> caseInfoList, long templateId);

        Task<TData> UploadFiles(HttpPostedFileBase file, string saveDirectoryName);
    }
}
