using Cappuccino.Entity;

namespace Cappuccino.IBLL
{
    public interface ISysLogLogonService : IBaseService<SysLogLogonEntity>
    {
        /// <summary>
        /// 写入登录日志
        /// </summary>
        /// <param name="logLogon"></param>
        /// <returns></returns>
        int WriteDbLog(SysLogLogonEntity logLogon);
    }
}
