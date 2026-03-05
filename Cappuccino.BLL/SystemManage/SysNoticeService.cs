using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;

namespace Cappuccino.BLL
{
    public class SysNoticeService : BaseService<SysNoticeEntity>, ISysNoticeService
    {
        private readonly ISysNoticeDao _noticeDao;

        #region 依赖注入
        public SysNoticeService(ISysNoticeDao noticeDao)
        {
            _noticeDao = noticeDao;
            base.CurrentDao = noticeDao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

        public SysNoticeEntity GetByKey(int id)
        {
            return _noticeDao.GetByKey(id);
        }
    }
}
