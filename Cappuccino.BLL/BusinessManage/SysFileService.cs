using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Helper;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;

namespace Cappuccino.BLL
{
    public class SysFileService : BaseService<SysFileEntity>, ISysFileService
    {
        private ISysFileDao _sysFileDao;

        #region 依赖注入
        public SysFileService(ISysFileDao sysFileDao)
        {
            _sysFileDao = sysFileDao;
            base.CurrentDao = sysFileDao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

        public async Task<int> SaveFileAsync(SysFileEntity entity)
        {
            entity.Create();
            return await InsertAsync(entity);
        }

        public List<string> GetFilePathById(long objectId)
        {
            return _sysFileDao.GetList(x => x.ObjectId == objectId).Select(x => x.FilePath).ToList();
        }
    }
}
