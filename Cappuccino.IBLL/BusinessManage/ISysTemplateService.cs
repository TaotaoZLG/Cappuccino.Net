using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Cappuccino.Entity;
using Cappuccino.Model;

namespace Cappuccino.IBLL
{
    public interface ISysTemplateService : IBaseService<SysTemplateEntity>
    {
        /// <summary>
        /// 获取最大排序码
        /// </summary>
        /// <returns></returns>
        int GetMaxSortCode();
    }
}
