using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Util;
using Cappuccino.Entity;

namespace Cappuccino.IBLL
{
    public interface ISysFileProcessiongService : IBaseService<SysCaseInfoEntity>
    {
        /// <summary>
        /// 压缩文件处理（解压→归档→OCR→生成Excel→打包→清理）
        /// </summary>
        /// <param name="file"></param>
        /// <param name="extractRule"></param>
        /// <param name="processType"></param>
        /// <param name="batchId"></param>
        /// <param name="progressAction"></param>
        /// <returns></returns>
        Task<TData<string>> ProcessCompressFileAsync(HttpPostedFileBase file, int extractRule, int processType, string batchId, Action<ProcessProgress> progressAction);

        Task<TData<string>> ProcessCompressFileAsync2(HttpPostedFileBase file, int extractRule, int processType, string batchId, Action<ProcessProgress> progressAction);
    }
}
