using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;

namespace Cappuccino.BLL
{
    public class SysDictTypeService : BaseService<SysDictTypeEntity>, ISysDictTypeService
    {
        #region 依赖注入
        ISysDictTypeDao dao;
        public SysDictTypeService(ISysDictTypeDao dao)
        {
            this.dao = dao;
            base.CurrentDao = dao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

    }
}
