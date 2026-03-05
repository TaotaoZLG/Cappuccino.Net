using System.Linq;
using Cappuccino.Common.Extensions;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;

namespace Cappuccino.BLL
{
    public class SysDictDetailService : BaseService<SysDictDetailEntity>, ISysDictDetailService
    {
        private readonly ISysDictDetailDao _dictDetailDao;

        #region 依赖注入
        public SysDictDetailService(ISysDictDetailDao dictDetailDao)
        {
            _dictDetailDao = dictDetailDao;
            base.CurrentDao = dictDetailDao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

        /// <summary>
        /// 获取最大的排序号
        /// </summary>
        /// <returns></returns>
        public int GetMaxSortCode(int dictId)
        {
            var result = _dictDetailDao.ExecuteSqlQuery<int?>($"SELECT MAX(SortCode) FROM SysDictDetail WHERE DictId = {dictId}").FirstOrDefault();
            int maxSortCode = result.ParseToInt();
            maxSortCode++;
            return maxSortCode;
        }
    }
}
