using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Helper;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;

namespace Cappuccino.BLL
{
    public class SysTemplateService : BaseService<SysTemplateEntity> , ISysTemplateService
    {
    private ISysTemplateDao _templateDao;

        #region 依赖注入
        public SysTemplateService(ISysTemplateDao templateDao)
        {
            _templateDao = templateDao;
            base.CurrentDao = templateDao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

        /// <summary>
        /// 获取最大的排序号
        /// </summary>
        /// <returns></returns>
        public int GetMaxSortCode()
        {
            var result = _templateDao.ExecuteSqlQuery<int?>("SELECT MAX(SortCode) FROM SysTemplate").FirstOrDefault();
            int maxSortCode = result.ParseToInt();
            maxSortCode++;
            return maxSortCode;
        }
    }
}
