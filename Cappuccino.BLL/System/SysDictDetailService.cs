using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;

namespace Cappuccino.BLL
{
    public class SysDictDetailService : BaseService<SysDictDetailEntity>, ISysDictDetailService
    {
        private readonly ISysDictDetailDao dao;

        #region 依赖注入
        public SysDictDetailService(ISysDictDetailDao dao)
        {
            this.dao = dao;
            base.CurrentDao = dao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

    }
}
