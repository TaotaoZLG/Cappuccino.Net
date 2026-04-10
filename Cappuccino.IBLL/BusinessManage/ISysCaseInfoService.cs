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

        /// <summary>
        /// 上传压缩包文件，解析文件并根据文件名关联案件数据（文件命名规则：姓名_卡号），可选将文件保存到指定目录
        /// </summary>
        /// <param name="file"></param>
        /// <param name="saveDirectoryName"></param>
        /// <returns></returns>
        Task<TData> UploadFiles(HttpPostedFileBase file, string saveDirectoryName);

        /// <summary>
        /// 下载文件（根据案件列表查询关联的文件信息，打包成Zip并返回Zip虚拟路径）
        /// </summary>
        /// <param name="caseInfoList"></param>
        /// <returns></returns>
        Task<TData<string>> DownloadFiles(List<SysCaseInfoEntity> caseInfoList);
    }
}
