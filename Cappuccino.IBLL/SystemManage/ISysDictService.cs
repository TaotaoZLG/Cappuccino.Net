using System.Collections.Generic;
using Cappuccino.Entity;
using Cappuccino.Model;
namespace Cappuccino.IBLL
{
    public interface ISysDictService : IBaseService<SysDictEntity>
    {
        int GetMaxSortCode();

        List<DataDictInfo> GetDataDictList();
    }
}
