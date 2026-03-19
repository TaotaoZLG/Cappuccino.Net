using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Util;
using Cappuccino.Entity;
using Cappuccino.Model;

namespace Cappuccino.IBLL
{
    public interface ISysCaseInfoService : IBaseService<SysCaseInfoEntity>
    {
        /// <summary>
        /// 生成诉状
        /// </summary>
        /// <param name="viewModel">查询条件视图模型</param>
        /// <param name="idsStr">选中行ID字符串（逗号分隔）</param>
        /// <param name="templateId">模板ID</param>
        /// <returns>ZIP文件内存流</returns>
        Task<TData> IndictmentAsync(List<SysCaseInfoEntity> caseInfoList, string idsStr, long templateId);
    }
}
