using Cappuccino.Entity;
namespace Cappuccino.IBLL
{
    public interface ISysNoticeService : IBaseService<SysNoticeEntity>
    {
        SysNoticeEntity GetByKey(int id);
    }
}
