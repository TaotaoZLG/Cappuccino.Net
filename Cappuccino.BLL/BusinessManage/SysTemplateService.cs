using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Util;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;
using Cappuccino.Model;

namespace Cappuccino.BLL
{
    public class SysTemplateService : BaseService<SysTemplateEntity>, ISysTemplateService
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

        public SysTemplateEntity GetTemplateById(long templateId)
        {
            return _templateDao.GetList(x => x.Id == templateId).FirstOrDefault();
        }
    }
}
