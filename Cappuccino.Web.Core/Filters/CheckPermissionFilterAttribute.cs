using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
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
                SysUserEntity userEntity = CacheManager.Get<SysUserEntity>(list[0]);
                if (userEntity != null)
                {                    
                    // 0为永久key
                    if (list[1] == "0")
                    {
                        CacheManager.Set(list[0], userEntity, new TimeSpan(10, 0, 0, 0));
                    }
                    // 1为滑动key（30分钟过期，每次访问续期）
                    else if (list[1] == "1")
                    {
                        CookieHelper.Set(KeyManager.IsMember, DESUtils.Encrypt(list.ToJson()), 30);
                        CacheManager.Set(list[0], userEntity, new TimeSpan(0, 30, 0));
                    }
                    else
                    {
                        ToLogin(filterContext);
                        return;
                    }
                }
                else
                {
                    ToLogin(filterContext);
                    return;
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
                var container = CacheManager.Get<IContainer>(KeyManager.AutofacContainer);
                ISysActionService sysActionService = container.Resolve<ISysActionService>();

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
            bool isAjaxRequst = filterContext.HttpContext.Request.IsAjaxRequest();
            if (isAjaxRequst)
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
                //var routeValues = new RouteValueDictionary
                //{
                //    { "controller", "Account" },
                //    { "action", "Login" }
                //};

                //// 设置重定向结果
                //filterContext.Result = new RedirectToRouteResult(routeValues);
            }
        }

        private static void NoPermission(AuthorizationContext filterContext)
        {
            bool isAjaxRequst = filterContext.HttpContext.Request.IsAjaxRequest();
            if (isAjaxRequst)
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