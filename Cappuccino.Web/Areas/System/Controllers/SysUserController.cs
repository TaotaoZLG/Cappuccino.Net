using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cappuccino.BLL;
using Cappuccino.Common;
using Cappuccino.Common.Enum;
using Cappuccino.Common.Util;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.Model;
using Cappuccino.Web.Attributes;
using Cappuccino.Web.Core;
using Cappuccino.Web.Models;

namespace Cappuccino.Web.Areas.System.Controllers
{
    public class SysUserController : BaseController
    {
        private readonly ISysUserService _sysUserService;
        private readonly ISysRoleService _sysRoleService;

        public SysUserController(ISysUserService sysUserService, ISysRoleService sysRoleService)
        {
            _sysUserService = sysUserService;
            _sysRoleService = sysRoleService;
            this.AddDisposableObject(_sysUserService);
            this.AddDisposableObject(_sysRoleService);
        }

        public SelectList RoleSelectList { get { return new SelectList(_sysRoleService.GetList(x => true).Select(x => new { x.Id, x.Name }), "Id", "Name"); } }

        #region 视图
        [CheckPermission("system.user.list")]
        public override ActionResult Index()
        {
            base.Index();
            return View();
        }

        [HttpGet, CheckPermission("system.user.create")]
        public ActionResult Create()
        {
            ViewBag.UploadFileSize = ConfigUtils.AppSetting.GetValue("UploadFileByImgSize");
            ViewBag.UploadFileType = ConfigUtils.AppSetting.GetValue("UploadFileByImgType");
            ViewBag.RoleSelectList = RoleSelectList;
            return View();
        }

        [HttpGet, CheckPermission("system.user.edit")]
        public ActionResult Edit(int id)
        {
            ViewBag.UploadFileSize = ConfigUtils.AppSetting.GetValue("UploadFileByImgSize");
            ViewBag.UploadFileType = ConfigUtils.AppSetting.GetValue("UploadFileByImgType");
            var entity = _sysUserService.GetList(x => x.Id == id).FirstOrDefault();
            ViewBag.RoleSelectList = RoleSelectList;
            var viewModel = entity.EntityMap();
            if (viewModel.SysRoles.Count != 0)
            {
                viewModel.RoleIds = string.Join(",", viewModel.SysRoles.Select(x => x.Id.ToString()).ToArray());
            }
            return View(viewModel);
        }

        [HttpGet, CheckPermission("system.user.assign")]
        public ActionResult Assign(int id)
        {
            ViewBag.UserId = id;
            return View();
        }

        public ActionResult Portrait()
        {
            return View();
        }
        #endregion

        #region 提交数据
        [HttpPost, CheckPermission("system.user.create")]
        [LogOperate(Title = "新增用户", BusinessType = (int)OperateType.Add)]
        public ActionResult Create(SysUserModel viewModel)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return WriteError("实体验证失败");
                }
                var user = _sysUserService.GetList(x => x.UserName == viewModel.UserName).FirstOrDefault();
                if (user != null)
                {
                    return WriteError("该账号已存在");
                }
                string salt = VerifyCodeUtils.CreateVerifyCode(5);
                string passwordHash = Md5Utils.EncryptTo32(salt + ConfigUtils.AppSetting.GetValue("InitUserPwd"));
                SysUserEntity entity = viewModel.EntityMap();
                entity.CreateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
                entity.CreateTime = DateTime.Now;
                entity.UpdateTime = DateTime.Now;
                entity.PasswordSalt = salt;
                entity.PasswordHash = passwordHash;
                if (!string.IsNullOrEmpty(viewModel.RoleIds))
                {
                    var RoleIdsArray = Array.ConvertAll(viewModel.RoleIds.Split(','), s => int.Parse(s));
                    var roleList = _sysRoleService.GetList(x => RoleIdsArray.Contains(x.Id)).ToList();
                    entity.SysRoles = roleList;
                }
                _sysUserService.Insert(entity);
                return WriteSuccess();
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.user.edit")]
        [LogOperate(Title = "编辑用户", BusinessType = (int)OperateType.Update)]
        public ActionResult Edit(int id, SysUserModel viewModel)
        {
            if (ModelState.IsValid == false)
            {
                return WriteError("实体验证失败");
            }
            var user = _sysUserService.GetList(x => x.UserName == viewModel.UserName && x.Id != id).FirstOrDefault();
            if (user != null)
            {
                return WriteError("该账号已存在");
            }
            //获取角色
            var roleList = new List<SysRoleEntity>();
            if (!string.IsNullOrEmpty(viewModel.RoleIds))
            {
                var RoleIdsArray = Array.ConvertAll(viewModel.RoleIds.Split(','), s => int.Parse(s));
                roleList = _sysRoleService.GetList(x => RoleIdsArray.Contains(x.Id)).ToList();
            }
            //赋值
            var entity = _sysUserService.GetList(x => x.Id == id).FirstOrDefault();
            entity.SysRoles.Clear();
            foreach (var item in roleList)
            {
                entity.SysRoles.Add(item);
            }
            entity.UserName = viewModel.UserName;
            entity.NickName = viewModel.NickName;
            entity.DepartmentId = viewModel.DepartmentId;
            entity.HeadIcon = viewModel.HeadIcon;
            entity.MobilePhone = viewModel.MobilePhone;
            entity.Email = viewModel.Email;
            entity.EnabledMark = (int)viewModel.EnabledMark;
            entity.MobilePhone = viewModel.MobilePhone;
            entity.Email = viewModel.Email;
            entity.UpdateTime = DateTime.Now;
            entity.UpdateUserId = UserManager.GetCurrentUserInfo().Id;
            _sysUserService.Update(entity);
            return WriteSuccess();
        }

        [HttpPost, CheckPermission("system.user.delete")]
        [LogOperate(Title = "删除用户", BusinessType = (int)OperateType.Delete)]
        public ActionResult Delete(int id)
        {
            try
            {
                _sysUserService.DeleteBy(x => x.Id == id);
                return WriteSuccess("数据删除成功");
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.user.batchDel")]
        [LogOperate(Title = "批量删除用户", BusinessType = (int)OperateType.Delete)]
        public ActionResult BatchDel(string idsStr)
        {
            try
            {
                var idsArray = idsStr.Substring(0, idsStr.Length).Split(',');
                int[] ids = Array.ConvertAll<string, int>(idsArray, int.Parse);
                var result = _sysUserService.DeleteBy(x => ids.Contains(x.Id)) > 0 ? WriteSuccess("数据删除成功") : WriteError("数据删除失败");
                return result;
            }
            catch (Exception ex)
            {
                return WriteError(ex);
            }
        }

        [HttpPost, CheckPermission("system.user.edit")]
        [LogOperate(Title = "禁用用户", BusinessType = (int)OperateType.Authorize)]
        public ActionResult UpdateEnabledMark(int id, int enabledMark)
        {
            SysUserEntity entity = new SysUserEntity
            {
                Id = id,
                EnabledMark = enabledMark,
                UpdateTime = DateTime.Now,
                UpdateUserId = UserManager.GetCurrentUserInfo().Id
            };
            _sysUserService.Update(entity, new string[] { "EnabledMark", "UpdateTime", "UpdateUserId" });
            return WriteSuccess("更新成功");
        }

        [HttpPost, CheckPermission("system.user.initPwd")]
        [LogOperate(Title = "重置密码", BusinessType = (int)OperateType.Update)]
        public ActionResult InitPwd(int id)
        {
            string salt = VerifyCodeUtils.CreateVerifyCode(5);
            string pwd = ConfigUtils.AppSetting.GetValue("InitUserPwd");
            string passwordHash = Md5Utils.EncryptTo32(salt + pwd);
            SysUserEntity entity = new SysUserEntity
            {
                Id = id,
                PasswordSalt = salt,
                PasswordHash = passwordHash,
                UpdateTime = DateTime.Now,
                UpdateUserId = UserManager.GetCurrentUserInfo().Id
            };
            _sysUserService.Update(entity, new string[] { "PasswordSalt", "PasswordHash", "UpdateTime", "UpdateUserId" });
            return WriteSuccess("重置密码成功，新密码:" + pwd);
        }

        public ActionResult UploadPortrait(int id, string portraitUrl)
        {
            SysUserEntity entity = new SysUserEntity
            {
                Id = id,
                HeadIcon = portraitUrl,
                UpdateTime = DateTime.Now,
                UpdateUserId = UserManager.GetCurrentUserInfo().Id
            };
            _sysUserService.Update(entity, new string[] { "HeadIcon", "UpdateTime", "UpdateUserId" });
            return WriteSuccess("修改头像成功");
        }
        #endregion

        #region 获取数据
        [CheckPermission("system.user.list")]
        public JsonResult GetList(SysUserModel viewModel, PageInfo pageInfo)
        {
            QueryCollection queries = new QueryCollection();
            if (!string.IsNullOrEmpty(viewModel.UserName))
            {
                queries.Add(new Query { Name = "UserName", Operator = Query.Operators.Contains, Value = viewModel.UserName });
            }
            if (!string.IsNullOrEmpty(viewModel.NickName))
            {
                queries.Add(new Query { Name = "NickName", Operator = Query.Operators.Contains, Value = viewModel.NickName });
            }
            if (viewModel.DepartmentId != null)
            {
                queries.Add(new Query { Name = "DepartmentId", Operator = Query.Operators.Equal, Value = viewModel.DepartmentId });
            }

            var list = _sysUserService.GetListByPage(queries.AsExpression<SysUserEntity>(), pageInfo.Field, pageInfo.Order, pageInfo.Limit, pageInfo.Page, out int totalCount, x => x.Department, x => x.SysRoles).Select(x => new
            {
                x.Id,
                x.UserName,
                x.NickName,
                x.HeadIcon,
                x.MobilePhone,
                x.Email,
                x.EnabledMark,
                DepartmentName = x.Department.Name,
                RoleNames = x.SysRoles.Select(r => r.Name).ToList()
            }).ToList();
            return Json(Pager.Paging(list, totalCount), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}