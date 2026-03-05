using System;
using Cappuccino.Common.Net;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;
using Cappuccino.Web.Core;

namespace Cappuccino.BLL
{
    public class SysLogLogonService : BaseService<SysLogLogonEntity>, ISysLogLogonService
    {
        #region 依赖注入
        ISysLogLogonDao dao;
        public SysLogLogonService(ISysLogLogonDao dao)
        {
            this.dao = dao;
            base.CurrentDao = dao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion


        /// <summary>
        /// 写入登录日志
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int WriteLogonLog(SysLogLogonEntity logLogon)
        {
            logLogon.IPAddress = NetHelper.GetIp;
            logLogon.IPAddressName = NetHelper.GetIpLocation(logLogon.IPAddress);
            logLogon.CreateTime = DateTime.Now;
            logLogon.CreateUserId = UserManager.GetCurrentUserInfo()?.Id ?? 0;
            return Insert(logLogon);
        }
    }
}
