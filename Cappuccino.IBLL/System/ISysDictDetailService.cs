using Cappuccino.Entity;

namespace Cappuccino.IBLL
{
    public interface ISysDictDetailService : IBaseService<SysDictDetailEntity>
    {
        int GetMaxSortCode(int dictId);
    }
}
