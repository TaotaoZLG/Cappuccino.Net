using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;

namespace Cappuccino.BLL
{
    public class SysNoticeService : BaseService<SysNoticeEntity>, ISysNoticeService
    {
        //private readonly ISysDictDao dao;

        #region 依赖注入
        //public SysConfigService(ISysDictDao dao)
        //{
        //    this.dao = dao;
        //    base.CurrentDao = dao;
        //    this.AddDisposableObject(this.CurrentDao);
        //}
        #endregion

    }
}
