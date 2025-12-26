using Cappuccino.Entity;
namespace Cappuccino.IDAL
{
    public interface ISysNoticeDao : IBaseDao<SysNoticeEntity>
    {
        SysNoticeEntity GetByKey(int id);
    }
}
