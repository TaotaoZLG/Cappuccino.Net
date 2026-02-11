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
        Task<string> ProcessCompressFileAsync(HttpPostedFileBase file, CancellationToken cancellationToken, IProgress<ProcessProgress> progress);
    }
}
