using System.Collections.Generic;
using System.Linq;
using Cappuccino.Common.Enum;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;
using Cappuccino.Model;

namespace Cappuccino.BLL
{
    public class SysDepartmentService : BaseService<SysDepartmentEntity>, ISysDepartmentService
    {
        #region 依赖注入
        ISysDepartmentDao _dao;
        public SysDepartmentService(ISysDepartmentDao dao)
        {
            this._dao = dao;
            base.CurrentDao = dao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion
    }
}
