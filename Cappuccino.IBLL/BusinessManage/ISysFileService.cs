using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Entity;

namespace Cappuccino.IBLL
{
    public interface ISysFileService: IBaseService<SysFileEntity>
    {
        Task<int> SaveFileAsync(SysFileEntity entity);

        /// <summary>
        /// 获取对象关联的文件路径列表
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        List<string> GetFilePathById(long objectId);
    }
}
