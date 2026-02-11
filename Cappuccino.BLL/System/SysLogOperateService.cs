using System;
using System.Threading.Tasks;
using Cappuccino.Common.Net;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;

namespace Cappuccino.BLL.System
{
    public class SysLogOperateService : BaseService<SysLogOperateEntity>, ISysLogOperateService
    {
        #region 依赖注入
        private ISysLogOperateDao _dao;

        public SysLogOperateService(ISysLogOperateDao dao)
        {
            _dao = dao;
            base.CurrentDao = dao;
            AddDisposableObject(CurrentDao);
        }
        #endregion

        /// <summary>
        /// 写入操作日志（自动填充IP、时间等公共字段）
        /// </summary>
        public int WriteOperateLog(SysLogOperateEntity logOperate)
        {
            // 自动填充公共信息
            logOperate.IPAddress = NetHelper.GetIp; // 复用现有工具类获取IP
            logOperate.IPAddressName = NetHelper.GetIpLocation(logOperate.IPAddress); // 获取IP所在地
            logOperate.CreateTime = DateTime.Now;

            return Insert(logOperate);
        }

        /// <summary>
        /// 异步写入操作日志
        /// </summary>
        /// <param name="logOperate">操作日志实体</param>
        /// <returns>影响的行数</returns>
        public async Task<int> WriteOperateLogAsync(SysLogOperateEntity logOperate)
        {
            logOperate.IPAddress = NetHelper.GetIp; // 复用现有工具类获取IP

            // 如果NetHelper有异步版本的IP定位方法，建议使用：
            // logOperate.IPAddressName = await NetHelper.GetIpLocationAsync(logOperate.IPAddress);

            // 若只有同步版本，保持原有调用（IO操作建议最终改为异步）
            logOperate.IPAddressName = NetHelper.GetIpLocation(logOperate.IPAddress);

            logOperate.CreateTime = DateTime.Now;

            return await InsertAsync(logOperate);
        }
    }
}