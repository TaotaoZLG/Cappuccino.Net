using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;

namespace Cappuccino.BLL
{
    public class SysDictService : BaseService<SysDictEntity>, ISysDictService
    {
        #region 依赖注入
        ISysDictDao dao;
        public SysDictService(ISysDictDao dao)
        {
            this.dao = dao;
            base.CurrentDao = dao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

    }
}
