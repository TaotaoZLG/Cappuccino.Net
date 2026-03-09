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
    public class SysFileService : BaseService<SysFileEntity> , ISysFileService
    {
    private ISysFileDao _fileDao;

        #region 依赖注入
        public SysFileService(ISysFileDao fileDao)
        {
            _fileDao = fileDao;
            base.CurrentDao = fileDao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

        /// <summary>
        /// 异步写入
        /// </summary>
        /// <param name="logOperate">操作日志实体</param>
        /// <returns>影响的行数</returns>
        public async Task<int> SaveFileAsync(SysFileEntity entity)
        {
            entity.Id = IdGeneratorHelper.Instance.GetId();
            return await InsertAsync(entity);
        }
    }
}
