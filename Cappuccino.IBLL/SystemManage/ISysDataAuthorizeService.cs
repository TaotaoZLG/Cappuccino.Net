using System.Collections.Generic;
using Cappuccino.Entity;
using Cappuccino.Model;

namespace Cappuccino.IBLL
{
    public interface ISysDataAuthorizeService : IBaseService<SysDataAuthorizeEntity>
    {
        /// <summary>
        /// 获取用户对指定 DataType 的生效数据 Id 集合（返回 HashSet，若为空表示无授权，调用方可按策略处理）
        /// 如果 dataType 为 null（默认），方法将返回“用户所在部门及其子部门下的用户”的用户 Id 集合（DataType=3）。
        /// DataType: 1-机构，2-部门，3-用户
        /// userId 可省略：服务内部会使用当前登录用户 UserManager.GetCurrentUserInfo().Id
        /// </summary>
        HashSet<long> GetEffectiveDataIdsForUser(long? userId = null, int? dataType = null);

        /// <summary>
        /// 清除缓存（赋权后可以调用以保证即时生效）
        /// 可省略 userId：默认针对当前登录用户
        /// </summary>
        void ClearCache(long? userId = null, int? dataType = null);
    }
}
