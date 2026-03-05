using Cappuccino.Entity;
namespace Cappuccino.IBLL
{
    public interface ISysDictService : IBaseService<SysDictEntity>
    {
        int GetMaxSortCode();
    }
}
