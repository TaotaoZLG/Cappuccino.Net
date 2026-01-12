using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;

namespace Cappuccino.BLL
{
    public class SysConfigService : BaseService<SysConfigEntity>, ISysConfigService
    {
        private readonly ISysConfigDao _configDao;

        #region 依赖注入
        public SysConfigService(ISysConfigDao configDao)
        {
            this._configDao = configDao;
            base.CurrentDao = configDao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

        public SysConfigEntity GetByConfig(string configKey)
        {
            return _configDao.GetByConfig(configKey);
        }
    }
}
