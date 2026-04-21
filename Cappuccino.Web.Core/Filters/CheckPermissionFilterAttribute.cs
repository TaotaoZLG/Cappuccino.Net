using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Cappuccino.Common;
using Cappuccino.Common.Caching;
using Cappuccino.Common.Enum;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Util;
using Cappuccino.Entity;
using Cappuccino.IBLL;

namespace Cappuccino.Web.Core.Filters
{
    public class CheckPermissionFilterAttribute : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            // 判断是否有贴跳过登录检查的特性标签(控制器或方法)
            if (filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(SkipCheckLogin), false) || filterContext.ActionDescriptor.IsDefined(typeof(SkipCheckLogin), false))
            {
                return;
            }

            // 登录状态验证
            var userCookie = CookieHelper.Get(KeyManager.IsMember);
            if (!string.IsNullOrEmpty(userCookie))
            {
                List<string> list = DESUtils.Decrypt(userCookie).ToList<string>();
                if (list == null || list.Count() != 2)
                {
                    ToLogin(filterContext);
                    return;
                }

                string userCacheKey = list[0];
                string expireFlag = list[1];

                SysUserEntity userInfo = CacheManager.Get<SysUserEntity>(userCacheKey);
                if (userInfo == null)
                {
                    ToLogin(filterContext);
                    return;
                }
                else
                {
                    TimeSpan expiresTime = TimeSpan.Zero;
                    string encryptData = DESUtils.Encrypt(list.ToJson());

                    // 0为永久key（绝对过期，过期时间为10天（相当于永久，除非用户清除cookie或缓存），登录后续期（每次访问续期））
                    if (expireFlag == "0")
                    {
                        expiresTime = TimeSpan.FromDays(10);

                        CookieHelper.Set(KeyManager.IsMember, encryptData, expiresTime);
                        CacheManager.Set(userCacheKey, userInfo, expiresTime, CacheExpirationTypeEnum.Absolute);
                    }
                    // 1为滑动key（滑动过期，过期时间为30分钟，每次访问续期）
                    else if (expireFlag == "1")
                    {
                        expiresTime = TimeSpan.FromMinutes(30);

                        CookieHelper.Set(KeyManager.IsMember, encryptData, expiresTime);
                        CacheManager.Set(userCacheKey, userInfo, expiresTime, CacheExpirationTypeEnum.Sliding);
                    }
                    else
                    {
                        ToLogin(filterContext);
                        return;
                    }
                }                
            }
            else
            {
                ToLogin(filterContext);
                return;
            }

            // 判断是否为超级管理员（IsSystem=1），若是则直接跳过权限检查
            var currentUser = UserManager.GetCurrentUserInfo();
            if (currentUser != null && currentUser.IsSystem == 1)
            {
                return; // 超级管理员直接通过验证
            }

            // 获得当前要执行的Action上标注的CheckPermissionAttribute实例对象，执行权限验证
            CheckPermission[] permAtts = (CheckPermission[])filterContext.ActionDescriptor
                .GetCustomAttributes(typeof(CheckPermission), false);
            if (permAtts?.Length > 0)
            {
                ISysActionService sysActionService = GlobalContext.Container.Resolve<ISysActionService>();

                // 检查是否有权限
                foreach (var permAtt in permAtts)
                {
                    // 判断当前登录用户是否具有permAtt.Permission权限
                    if (!sysActionService.HasPermission(currentUser.Id, permAtt.Permission))
                    {
                        NoPermission(filterContext);
                        return;
                    }
                }
            }
        }

        private static void ToLogin(AuthorizationContext filterContext)
        {
            bool isAjaxRequest = filterContext.HttpContext.Request.IsAjaxRequest();
            if (isAjaxRequest)
            {
                JsonResult json = new JsonResult();
                json.Data = new { status = (int)AjaxStateEnum.NoLogin, msg = "您未登录或登录已失效，请重新登录" };
                json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                filterContext.Result = json;
            }
            else
            {
                ViewResult view = new ViewResult();
                view.ViewName = "/Views/Shared/Tip.cshtml";
                filterContext.Result = view;
            }
        }

        private static void NoPermission(AuthorizationContext filterContext)
        {
            bool isAjaxRequest = filterContext.HttpContext.Request.IsAjaxRequest();
            if (isAjaxRequest)
            {
                JsonResult json = new JsonResult
                {
                    Data = new { status = (int)AjaxStateEnum.NoPermission, msg = "您没有执行此操作的权限" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                filterContext.Result = json;
            }
            else
            {
                ViewResult view = new ViewResult
                {
                    ViewName = "/Views/Shared/Error403.cshtml"
                };
                filterContext.Result = view;
            }
        }
    }
}