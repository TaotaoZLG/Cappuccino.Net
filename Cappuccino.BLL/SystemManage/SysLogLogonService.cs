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
            string ip = NetHelper.GetIp;
            string IPAddressName = NetHelper.GetIpLocation(ip);
            string systemOs = NetHelper.GetSystemOs(null);
            string browser = NetHelper.GetBrowser(null);

            logLogon.IPAddress = ip;
            logLogon.IPAddressName = IPAddressName;
            logLogon.CreateUserId = UserManager.GetCurrentUserInfo()?.Id ?? 0;
            logLogon.SystemOs = systemOs;
            logLogon.Browser = browser;

            logLogon.Create();

            return Insert(logLogon);
        }
    }
}
