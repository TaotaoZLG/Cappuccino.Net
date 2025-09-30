using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using Cappuccino.Common.Enum;
using Cappuccino.Common.Util;
using Cappuccino.Entity;

namespace Cappuccino.DAL.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Cappuccino.DAL.EfDbContext>
    {
        public Configuration()
        {
            //数据
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(EfDbContext context)
        {
            #region 用户数据
            string salt = VerifyCodeUtils.CreateVerifyCode(5);
            string passwordHash = Md5Utils.EncryptTo32(salt + "123456");
            var sysUsers = new List<SysUserEntity>
            {
              new SysUserEntity{UserName="admin",NickName="超级管理员",HeadIcon="/Content/admin/images/avatar.jpg",PasswordSalt=salt,PasswordHash=passwordHash,
                          Email="admin@Cappuccino.com",EnabledMark=(int)EnabledMarkEnum.Valid,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
              new SysUserEntity{UserName="system",NickName="系统管理员",HeadIcon="/Content/admin/images/avatar.jpg",PasswordSalt=salt,PasswordHash=passwordHash,
                          Email="system@Cappuccino.com",EnabledMark=(int)EnabledMarkEnum.Invalid,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
              new SysUserEntity{UserName="user",NickName="普通用户",HeadIcon="/Content/admin/images/avatar.jpg",PasswordSalt=salt,PasswordHash=passwordHash,
                          Email="user@Cappuccino.com",EnabledMark=(int)EnabledMarkEnum.Invalid,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
              new SysUserEntity{UserName="test",NickName="测试用户",HeadIcon="/Content/admin/images/avatar.jpg",PasswordSalt=salt,PasswordHash=passwordHash,
                          Email="test@Cappuccino.com",EnabledMark=(int)EnabledMarkEnum.Invalid,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
            };
            sysUsers.ForEach(s => context.Set<SysUserEntity>().Add(s));
            context.SaveChanges();
            #endregion

            #region 角色数据
            var sysRoles = new List<SysRoleEntity>
            {
              new SysRoleEntity{  Name="超级管理员",Code="Administrator",EnabledMark=(int)EnabledMarkEnum.Valid,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
              new SysRoleEntity{  Name="系统管理员",Code="system",EnabledMark=(int)EnabledMarkEnum.Invalid,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
              new SysRoleEntity{  Name="普通用户",Code="user",EnabledMark=(int)EnabledMarkEnum.Invalid,Remark="只有查看页面的权限",CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
              new SysRoleEntity{  Name="测试用户",Code="test",EnabledMark=(int)EnabledMarkEnum.Invalid,Remark="用来测试的",CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},

            };
            sysRoles.ForEach(s => context.Set<SysRoleEntity>().Add(s));
            context.SaveChanges();
            #endregion

            #region 用户角色数据
            var adminUser = context.Set<SysUserEntity>().Single(s => s.UserName == "admin");
            var adminRole = context.Set<SysRoleEntity>().Single(s => s.Code == "Administrator");
            adminUser.SysRoles.Add(adminRole);
            var systemUser = context.Set<SysUserEntity>().Single(s => s.UserName == "system");
            var systemRole = context.Set<SysRoleEntity>().Single(s => s.Code == "system");
            systemUser.SysRoles.Add(systemRole);
            var user = context.Set<SysUserEntity>().Single(s => s.UserName == "user");
            var userRole = context.Set<SysRoleEntity>().Single(s => s.Code == "user");
            user.SysRoles.Add(userRole);
            var testUser = context.Set<SysUserEntity>().Single(s => s.UserName == "test");
            var testRole = context.Set<SysRoleEntity>().Single(s => s.Code == "test");
            testUser.SysRoles.Add(testRole);
            context.SaveChanges();
            #endregion

            #region 权限管理
            //目录
            var sysActionsByCatalog = new List<SysActionEntity>
            {
                 new SysActionEntity{Name="系统管理",Code="system",ParentId=0,Type=0,SortCode=1,
                 SysActionMenu=new SysActionMenuEntity{Icon="layui-icon-set-fill" },
                 CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1}
            };
            sysActionsByCatalog.ForEach(s => context.Set<SysActionEntity>().Add(s));
            context.SaveChanges();

            //菜单
            var systemMenu = context.Set<SysActionEntity>().Single(s => s.Code == "system");
            var sysActionByMenus = new List<SysActionEntity>
            {
                new SysActionEntity{Id=2,Name="用户管理",Code="system.user.view",ParentId=systemMenu.Id,Type=0,SortCode=1
                ,SysActionMenu=new SysActionMenuEntity{Icon="layui-icon-face-cry",Url="/System/SysUser" }
                ,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Id=3,Name="角色管理",Code="system.role.view",ParentId=systemMenu.Id,Type=0,SortCode=2
                ,SysActionMenu=new SysActionMenuEntity{Icon="layui-icon-face-cry",Url="/System/SysRole" }
                ,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Id=4,Name="菜单管理",Code="system.menu.view",ParentId=systemMenu.Id,Type=0,SortCode=3
                ,SysActionMenu=new SysActionMenuEntity{Icon="layui-icon-face-cry",Url="/System/SysAction" }
                ,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Id=5,Name="数据字典",Code="system.dict.view",ParentId=systemMenu.Id,Type=0,SortCode=4
                ,SysActionMenu=new SysActionMenuEntity{Icon="layui-icon-face-cry",Url="/System/SysDict" }
                ,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Id=6,Name="日志管理",Code="system.log",ParentId=systemMenu.Id,Type=0,SortCode=5
                ,SysActionMenu=new SysActionMenuEntity{Icon="layui-icon-face-cry"}
                ,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=systemMenu.Id,UpdateUserId=1},
            };
            sysActionByMenus.ForEach(s => context.Set<SysActionEntity>().Add(s));
            context.SaveChanges();

            //子级菜单
            var systemMenuSystemLog = context.Set<SysActionEntity>().Single(s => s.Code == "system.log");
            var sysActionByMenuSystemLogs = new List<SysActionEntity>
            {
                new SysActionEntity{Id=7,Name="登录日志",Code="system.loglogon.view",ParentId=systemMenuSystemLog.Id,Type=0,SortCode=6
                ,SysActionMenu=new SysActionMenuEntity{Icon="layui-icon-face-cry",Url="/System/SysLogLogon" }
                ,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
            };
            sysActionByMenuSystemLogs.ForEach(s => context.Set<SysActionEntity>().Add(s));
            context.SaveChanges();

            //按钮
            var systemMenuSystemUser = context.Set<SysActionEntity>().Single(s => s.Code == "system.user.view");
            var systemMenuSystemRole = context.Set<SysActionEntity>().Single(s => s.Code == "system.role.view");
            var systemMenuSystemAction = context.Set<SysActionEntity>().Single(s => s.Code == "system.menu.view");
            var systemMenuSystemDict = context.Set<SysActionEntity>().Single(s => s.Code == "system.dict.view");
            var systemMenuSystemLogLogon = context.Set<SysActionEntity>().Single(s => s.Code == "system.loglogon.view");
            var sysActionByButton = new List<SysActionEntity>
            {
                new SysActionEntity{Name="新增",Code="system.user.create",ParentId=systemMenuSystemUser.Id,Type=ActionTypeEnum.Button,SortCode=1,
                SysActionButton=new SysActionButtonEntity{ButtonCode="create",Location=PositionEnum.FormRightTop,ButtonClass="pear-btn pear-btn-primary pear-btn-md",ButtonIcon="layui-icon-add-1" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="编辑",Code="system.user.edit",ParentId=systemMenuSystemUser.Id,Type=ActionTypeEnum.Button,SortCode=2,
                SysActionButton=new SysActionButtonEntity{ButtonCode="edit",Location=PositionEnum.FormInside,ButtonClass="pear-btn pear-btn-primary pear-btn-sm",ButtonIcon="layui-icon-edit" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="删除",Code="system.user.delete",ParentId=systemMenuSystemUser.Id,Type=ActionTypeEnum.Button,SortCode=3,
                SysActionButton=new SysActionButtonEntity{ButtonCode="delete",Location=PositionEnum.FormInside,ButtonClass="pear-btn pear-btn-danger pear-btn-sm",ButtonIcon="layui-icon-delete" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="批量删除",Code="system.user.batchDel",ParentId=systemMenuSystemUser.Id,Type=ActionTypeEnum.Button,SortCode=4,
                SysActionButton=new SysActionButtonEntity{ButtonCode="batchDel",Location=PositionEnum.FormRightTop,ButtonClass="pear-btn pear-btn-danger pear-btn-md",ButtonIcon="layui-icon-delete" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="分配权限",Code="system.user.assign",ParentId=systemMenuSystemUser.Id,Type=ActionTypeEnum.Button,SortCode=5,
                SysActionButton=new SysActionButtonEntity{ButtonCode="assign",Location=PositionEnum.FormInside,ButtonClass="pear-btn pear-btn-warming pear-btn-sm",ButtonIcon="layui-icon-vercode" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="重置密码",Code="system.user.initPwd",ParentId=systemMenuSystemUser.Id,Type=ActionTypeEnum.Button,SortCode=6,
                SysActionButton=new SysActionButtonEntity{ButtonCode="initPwd",Location=PositionEnum.FormInside,ButtonClass="pear-btn pear-btn-warming pear-btn-sm",ButtonIcon="layui-icon-refresh" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},

                new SysActionEntity{Name="新增",Code="system.role.create",ParentId=systemMenuSystemRole.Id,Type=ActionTypeEnum.Button,SortCode=1,
                SysActionButton=new SysActionButtonEntity{ButtonCode="create",Location=PositionEnum.FormRightTop,ButtonClass="pear-btn pear-btn-primary pear-btn-md",ButtonIcon="layui-icon-add-1" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="编辑",Code="system.role.edit",ParentId=systemMenuSystemRole.Id,Type=ActionTypeEnum.Button,SortCode=2,
                SysActionButton=new SysActionButtonEntity{ButtonCode="edit",Location=PositionEnum.FormInside,ButtonClass="pear-btn pear-btn-primary pear-btn-sm",ButtonIcon="layui-icon-edit" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="删除",Code="system.role.delete",ParentId=systemMenuSystemRole.Id,Type=ActionTypeEnum.Button,SortCode=3,
                SysActionButton=new SysActionButtonEntity{ButtonCode="delete",Location=PositionEnum.FormInside,ButtonClass="pear-btn pear-btn-danger pear-btn-sm",ButtonIcon="layui-icon-delete" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="批量删除",Code="system.role.batchDel",ParentId=systemMenuSystemRole.Id,Type=ActionTypeEnum.Button,SortCode=4,
                SysActionButton=new SysActionButtonEntity{ButtonCode="batchDel",Location=PositionEnum.FormRightTop,ButtonClass="pear-btn pear-btn-danger pear-btn-md",ButtonIcon="layui-icon-delete" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="分配权限",Code="system.role.assign",ParentId=systemMenuSystemRole.Id,Type=ActionTypeEnum.Button,SortCode=5,
                SysActionButton=new SysActionButtonEntity{ButtonCode="assign",Location=PositionEnum.FormInside,ButtonClass="pear-btn pear-btn-warming pear-btn-sm",ButtonIcon="layui-icon-vercode" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},

                new SysActionEntity{Name="新增",Code="system.menu.create",ParentId=systemMenuSystemAction.Id,Type=ActionTypeEnum.Button,SortCode=1,
                SysActionButton=new SysActionButtonEntity{ButtonCode="create",Location=PositionEnum.FormRightTop,ButtonClass="pear-btn pear-btn-primary pear-btn-md",ButtonIcon="layui-icon-add-1" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="编辑",Code="system.menu.edit",ParentId=systemMenuSystemAction.Id,Type=ActionTypeEnum.Button,SortCode=2,
                SysActionButton=new SysActionButtonEntity{ButtonCode="edit",Location=PositionEnum.FormInside,ButtonClass="pear-btn pear-btn-primary pear-btn-sm",ButtonIcon="layui-icon-edit" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="删除",Code="system.menu.delete",ParentId=systemMenuSystemAction.Id,Type=ActionTypeEnum.Button,SortCode=3,
                SysActionButton=new SysActionButtonEntity{ButtonCode="delete",Location=PositionEnum.FormInside,ButtonClass="pear-btn pear-btn-danger pear-btn-sm",ButtonIcon="layui-icon-delete" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="批量删除",Code="system.menu.batchDel",ParentId=systemMenuSystemAction.Id,Type=ActionTypeEnum.Button,SortCode=4,
                SysActionButton=new SysActionButtonEntity{ButtonCode="batchDel",Location=PositionEnum.FormRightTop,ButtonClass="pear-btn pear-btn-danger pear-btn-md",ButtonIcon="layui-icon-delete" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},

                new SysActionEntity{Name="新增",Code="system.dict.create",ParentId=systemMenuSystemDict.Id,Type=ActionTypeEnum.Button,SortCode=1,
                SysActionButton=new SysActionButtonEntity{ButtonCode="create",Location=PositionEnum.FormRightTop,ButtonClass="pear-btn pear-btn-primary pear-btn-md",ButtonIcon="layui-icon-add-1" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="编辑",Code="system.dict.edit",ParentId=systemMenuSystemDict.Id,Type=ActionTypeEnum.Button,SortCode=2,
                SysActionButton=new SysActionButtonEntity{ButtonCode="edit",Location=PositionEnum.FormInside,ButtonClass="pear-btn pear-btn-primary pear-btn-sm",ButtonIcon="layui-icon-edit" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="删除",Code="system.dict.delete",ParentId=systemMenuSystemDict.Id,Type=ActionTypeEnum.Button,SortCode=3,
                SysActionButton=new SysActionButtonEntity{ButtonCode="delete",Location=PositionEnum.FormInside,ButtonClass="pear-btn pear-btn-danger pear-btn-sm",ButtonIcon="layui-icon-delete" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysActionEntity{Name="批量删除",Code="system.dict.batchDel",ParentId=systemMenuSystemDict.Id,Type=ActionTypeEnum.Button,SortCode=4,
                SysActionButton=new SysActionButtonEntity{ButtonCode="batchDel",Location=PositionEnum.FormRightTop,ButtonClass="pear-btn pear-btn-danger pear-btn-md",ButtonIcon="layui-icon-delete" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},

                new SysActionEntity{Id=26,Name="导出",Code="system.loglogon.export",ParentId=systemMenuSystemLogLogon.Id,Type=ActionTypeEnum.Button,SortCode=1,
                SysActionButton=new SysActionButtonEntity{ButtonCode="export",Location=PositionEnum.FormRightTop,ButtonClass="pear-btn pear-btn-primary pear-btn-md",ButtonIcon="layui-icon-export" },
                CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
            };

            sysActionByButton.ForEach(s => context.Set<SysActionEntity>().Add(s));
            context.SaveChanges();
            #endregion

            #region 分配权限
            //超级管理员
            adminRole = context.Set<SysRoleEntity>().Single(s => s.Code == "Administrator");
            var adminAction = context.Set<SysActionEntity>().ToList();
            adminAction.ForEach(x => adminRole.SysActions.Add(x));
            //普通用户
            userRole = context.Set<SysRoleEntity>().Single(s => s.Code == "user");
            adminAction = context.Set<SysActionEntity>().Where(x => x.Type == ActionTypeEnum.Menu).ToList();
            adminAction.ForEach(x => adminRole.SysActions.Add(x));
            context.SaveChanges();
            #endregion

            #region 数据字典
            var sysDicts = new List<SysDictEntity>
            {
                new SysDictEntity{Name="机构类型",Code="OrganizeCategory",SortCode=1,SysDictDetails=new List<SysDictDetailEntity> {
                    new SysDictDetailEntity{Name="公司",Code="Company",SortCode=1,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                    new SysDictDetailEntity{Name="部门",Code="Department",SortCode=2,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                    new SysDictDetailEntity{Name="小组",Code="WorkGroup",SortCode=3,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1}},
                    CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                new SysDictEntity{Name="性别",Code="Sex",SortCode=2,SysDictDetails=new List<SysDictDetailEntity> {
                    new SysDictDetailEntity{Name="男",Code="Male",SortCode=1,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
                    new SysDictDetailEntity{Name="女",Code="Female",SortCode=2,CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1}},
                    CreateTime=DateTime.Now,UpdateTime=DateTime.Now,CreateUserId=1,UpdateUserId=1},
            };
            sysDicts.ForEach(s => context.Set<SysDictEntity>().Add(s));

            context.SaveChanges();
            #endregion
        }
    }
}
