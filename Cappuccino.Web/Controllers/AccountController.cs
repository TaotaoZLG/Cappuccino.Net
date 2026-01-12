using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cappuccino.Common;
using Cappuccino.Common.Caching;
using Cappuccino.Common.Enum;
using Cappuccino.Common.Helper;
using Cappuccino.Common.Net;
using Cappuccino.Common.Util;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.Model;
using Cappuccino.Web.Attributes;
using Cappuccino.Web.Core;

namespace Cappuccino.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly ISysUserService _sysUserService;
        private readonly ISysLogLogonService _sysLogLogonService;

        public AccountController(ISysUserService sysUserService, ISysLogLogonService sysLogLogonService)
        {
            _sysUserService = sysUserService;
            _sysLogLogonService = sysLogLogonService;
        }

        #region 视图
        [SkipCheckLogin]
        public ActionResult Login()
        {
            LoginModel loginModel = new LoginModel()
            {
                LoginName = "admin",
                IsMember = true
            };

            if (Request.Cookies[KeyManager.IsMember] != null)
            {
                loginModel.IsMember = true;
            }
            return View(loginModel);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        [LogOperate(Title = "修改密码", BusinessType = (int)OperateType.Update)]
        public ActionResult ChangePassword()
        {
            ViewBag.UserName = UserManager.GetCurrentUserInfo().UserName;
            return View();
        }

        public ActionResult Person()
        {
            return View();
        }
        #endregion

        #region 提交数据
        [HttpPost, SkipCheckLogin]
        [LogOperate(Title = "登录", BusinessType = (int)OperateType.Login)]
        public ActionResult Login(LoginModel loginModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return WriteError("实体验证失败");
                }
                if (loginModel.VerifyCode.ToLower() != (string)TempData["verifyCode"])
                {
                    return WriteError("验证码失败");
                }
                bool result = _sysUserService.CheckLogin(loginModel.LoginName, loginModel.LoginPassword);
                if (result)
                {
                    SysUserEntity user = _sysUserService.GetList(x => x.UserName == loginModel.LoginName).FirstOrDefault();
                    string userLoginId = Guid.NewGuid().ToString();
                    // 若选择"记住登录"（IsMember为true），缓存10天，Cookie长期有效
                    if (loginModel.IsMember)
                    {
                        List<string> list = new List<string>
                        {
                            userLoginId,
                            "0"
                        };
                        CookieHelper.Set(KeyManager.IsMember, DESUtils.Encrypt(list.ToJson()));
                        CacheManager.Set(userLoginId, user, TimeSpan.FromDays(10)); // 10天过期
                    }
                    else
                    {
                        // 不记住登录，缓存30分钟，Cookie 30分钟过期
                        CookieHelper.Remove(KeyManager.IsMember);
                        List<string> list = new List<string>
                        {
                            userLoginId,
                            "1"
                        };
                        CookieHelper.Set(KeyManager.IsMember, DESUtils.Encrypt(list.ToJson()), 30); // 30分钟过期
                        CacheManager.Set(userLoginId, user, TimeSpan.FromMinutes(30));    // 30分钟过期
                    }
                    _sysLogLogonService.WriteLogonLog(new SysLogLogonEntity
                    {
                        LogType = OperateType.Login.ToString(),
                        Account = user.UserName,
                        RealName = user.NickName,
                        SystemOs = NetHelper.GetSystemOs(null),
                        Browser = NetHelper.GetBrowser(null),
                        Description = "登陆成功",
                    });
                    return WriteSuccess("登录成功");
                }
                else
                {
                    return WriteError("用户名或者密码错误");
                }
            }
            catch (Exception ex)
            {
                _sysLogLogonService.WriteLogonLog(new SysLogLogonEntity
                {
                    LogType = OperateType.Exception.ToString(),
                    Account = loginModel.LoginName,
                    RealName = loginModel.LoginName,
                    SystemOs = NetHelper.GetSystemOs(null),
                    Browser = NetHelper.GetBrowser(null),
                    Description = "登录失败，" + ex.Message
                });
                return WriteError(ex);
            }
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [LogOperate(Title = "退出系统", BusinessType = (int)OperateType.Exit)]
        public ActionResult Logout()
        {
            var user = UserManager.GetCurrentUserInfo();
            TempData["UserInfo"] = user; // 暂存用户信息到TempData

            _sysLogLogonService.WriteLogonLog(new SysLogLogonEntity
            {
                LogType = OperateType.Exit.ToString(),
                Account = user.UserName,
                RealName = user.NickName,
                Description = "安全退出系统",
            });
            CacheManager.Remove(UserManager.GetCurrentUserCacheId());
            CookieHelper.Remove(KeyManager.IsMember);
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [LogOperate(Title = "修改密码", BusinessType = (int)OperateType.Update)]
        public ActionResult ModifyUserPwd(ChangePasswordModel viewModel)
        {
            // 验证新密码与确认密码是否一致
            if (viewModel.Password != viewModel.Repassword)
            {
                return WriteError("新密码与确认密码不一致");
            }

            int userId = UserManager.GetCurrentUserInfo().Id;
            var result = WriteError("出现异常，密码修改失败");

            try
            {
                // 验证旧密码是否正确
                if (!_sysUserService.CheckLogin(viewModel.UserName, viewModel.OldPassword))
                {
                    return WriteError("旧密码不正确");
                }

                SysUserEntity currentUser = _sysUserService.GetList(x => x.Id == userId).FirstOrDefault();
                if (currentUser == null)
                {
                    return WriteError("用户信息不存在");
                }

                // 验证新密码是否和旧密码一致
                string newPwdHash = Md5Utils.EncryptTo32(currentUser.PasswordSalt + viewModel.Password);
                if (currentUser.PasswordHash == newPwdHash)
                {
                    return WriteError("密码未更改，新密码和原密码一致");
                }

                if (_sysUserService.ModifyUserPwd(userId, viewModel))
                {
                    string cookieValue = CookieHelper.Get(KeyManager.IsMember);
                    if (!string.IsNullOrEmpty(cookieValue))
                    {
                        List<string> list = DESUtils.Decrypt(cookieValue).ToList<string>();
                        if (list != null && list.Count == 2)
                        {
                            //获取缓存的用户信息
                            SysUserEntity userinfo = CacheManager.Get<SysUserEntity>(list[0]);
                            if (userinfo != null)
                            {
                                //删除缓存的用户信息
                                CacheManager.Remove(list[0]);
                                //更新缓存用户信息的KEY
                                list[0] = Guid.NewGuid().ToString();
                                if (list[1] == "0")  //记住密码（10天）
                                {
                                    CacheManager.Set(list[0], userinfo, TimeSpan.FromDays(10)); // 10天
                                    CookieHelper.Set(KeyManager.IsMember, DESUtils.Encrypt(list.ToJson()));
                                }
                                else if (list[1] == "1")   //不记住密码（30分钟）
                                {
                                    CacheManager.Set(list[0], userinfo, TimeSpan.FromMinutes(30));    // 30分钟
                                    CookieHelper.Set(KeyManager.IsMember, DESUtils.Encrypt(list.ToJson()), 30);
                                }
                            }
                        }
                    }
                    result = WriteSuccess("密码修改成功");
                }
            }
            catch (Exception ex)
            {
                result = WriteError("密码修改失败：" + ex.Message);
            }
            return result;
        }
        #endregion

        #region 获取数据
        /// <summary>
        /// 创建验证码
        /// </summary>
        /// <returns></returns>
        [SkipCheckLogin]
        public ActionResult CreateVerifyCode()
        {
            string verifyCode = VerifyCodeUtils.CreateVerifyCode(4);
            TempData["verifyCode"] = verifyCode.ToLower();
            return File(VerifyCodeUtils.GenerateImage(verifyCode), @"image/Gif");
        }
        #endregion
    }
}