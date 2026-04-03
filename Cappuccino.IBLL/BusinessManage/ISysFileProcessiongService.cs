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
        /// 压缩包文件处理
        /// 1.解压（压缩包名 = 根目录）
        /// 2.多级文件夹最后一级匹配
        /// 3.文件分类归档（3 个子文件夹）
        /// 4.图片过滤 → 生成归档 Word
        /// 5.地址截屏 Word → 提取第一张图 → OCR 识别
        /// 6.OCR 记录收集 → MiniExcel 导出 Excel
        /// 7.身份证解析（性别 + 生日）
        /// 8.全量数据 + 归档路径 + OCR 结果 → 数据库入库
        /// 9.打包导出（合并excel + OCR识别结果）
        /// 10.清理服务器文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="extractRule"></param>
        /// <param name="processType"></param>
        /// <param name="batchId"></param>
        /// <param name="progressAction"></param>
        /// <returns></returns>
        Task<TData<string>> ProcessCompressFileAsync(HttpPostedFileBase file, int extractRule, int processType, string batchId, Action<ProcessProgress> progressAction);
    }
}
