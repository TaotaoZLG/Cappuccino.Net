using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;

namespace Cappuccino.BLL
{
    public class SysDictService : BaseService<SysDictEntity>, ISysDictService
    {
        private readonly ISysDictDao dao;

        #region 依赖注入
        public SysDictService(ISysDictDao dao)
        {
            this.dao = dao;
            base.CurrentDao = dao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

    }
}
