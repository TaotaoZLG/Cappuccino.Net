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
    }
}
