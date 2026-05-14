using System.Collections.Generic;
using System.Linq;
using Cappuccino.DataAccess;
using Cappuccino.Entity;
using Cappuccino.IDAL;

namespace Cappuccino.DAL.System
{
    public class SysDictDao : BaseDao<SysDictEntity>, ISysDictDao
    {
        /// <summary>
        /// 获取数据字典列表
        /// </summary>
        /// <returns></returns>
        public List<SysDictEntity> GetDataDictList()
        { 
            return GetList(x => true).ToList();
        }
    }
}
