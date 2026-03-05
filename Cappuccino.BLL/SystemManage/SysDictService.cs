using System.Linq;
using Cappuccino.Common.Extensions;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;

namespace Cappuccino.BLL
{
    public class SysDictService : BaseService<SysDictEntity>, ISysDictService
    {
        private readonly ISysDictDao _dictDao;

        #region 依赖注入
        public SysDictService(ISysDictDao dictDao)
        {
            _dictDao = dictDao;
            base.CurrentDao = dictDao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

        /// <summary>
        /// 获取最大的排序号
        /// </summary>
        /// <returns></returns>
        public int GetMaxSortCode()
        {
            var result = _dictDao.ExecuteSqlQuery<int?>($"SELECT MAX(SortCode) FROM SysDict WHERE 1=1").FirstOrDefault();
            int maxSortCode = result.ParseToInt();
            maxSortCode++;
            return maxSortCode;
        }
    }
}
