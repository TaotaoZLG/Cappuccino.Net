
/*SysUser*/
-- 1. 唯一索引（已在EF映射中定义，校验+优化查询）
CREATE UNIQUE NONCLUSTERED INDEX IX_SysUser_UserName 
ON dbo.SysUser (UserName ASC);

-- 2. 排序+筛选联合索引（适配GetList分页查询：EnabledMark筛选 + CreateTime排序）
CREATE NONCLUSTERED INDEX IX_SysUser_EnabledMark_CreateTime 
ON dbo.SysUser (EnabledMark ASC, CreateTime DESC)
INCLUDE (NickName, Email, MobilePhone); -- 包含高频查询字段，避免回表

-- 3. 关联查询索引（用户-角色关联）
CREATE NONCLUSTERED INDEX IX_SysUser_Id_CreateTime 
ON dbo.SysUser (Id ASC, CreateTime DESC);

/*SysRole*/
CREATE UNIQUE NONCLUSTERED INDEX IX_SysRole_Code 
ON dbo.SysRole (Code ASC);

-- 2. 筛选+排序索引（角色列表查询）
CREATE NONCLUSTERED INDEX IX_SysRole_EnabledMark_CreateTime 
ON dbo.SysRole (EnabledMark ASC, CreateTime DESC)
INCLUDE (Name, Remark);

-- 3. 关联查询索引
CREATE NONCLUSTERED INDEX IX_SysRole_Id 
ON dbo.SysRole (Id ASC);

/*SysUserRole*/
CREATE NONCLUSTERED INDEX IX_SysUserRole_RoleId_UserId 
ON dbo.SysUserRole (RoleId ASC, UserId ASC);

-- 2. 正向索引（适配“用户查角色”场景）
CREATE NONCLUSTERED INDEX IX_SysUserRole_UserId_RoleId 
ON dbo.SysUserRole (UserId ASC, RoleId ASC);

/*SysAction*/
-- 1. 菜单树查询索引（ParentId + Type + SortCode 适配树形结构查询）
CREATE NONCLUSTERED INDEX IX_SysAction_ParentId_Type_SortCode 
ON dbo.SysAction (ParentId ASC, Type ASC, SortCode ASC)
INCLUDE (Name, Code, Url);

-- 2. 排序索引（权限列表查询）
CREATE NONCLUSTERED INDEX IX_SysAction_CreateTime 
ON dbo.SysAction (CreateTime DESC);

-- 3. 唯一索引（权限编码唯一）
CREATE UNIQUE NONCLUSTERED INDEX IX_SysAction_Code 
ON dbo.SysAction (Code ASC);

/*SysLogLogon*/
CREATE NONCLUSTERED INDEX IX_SysLogLogon_CreateTime_Account 
ON dbo.SysLogLogon (CreateTime DESC, Account ASC)
INCLUDE (LogType, IPAddress, Description);

/*SysLogOperate*/
CREATE NONCLUSTERED INDEX IX_SysLogOperate_CreateTime_Id 
ON dbo.SysLogOperate (CreateTime DESC, Id ASC)
INCLUDE (CreateUserId, ModuleName, OperateType, IPAddress, Remark);

/*SysRoleAction*/
CREATE NONCLUSTERED INDEX IX_SysRoleAction_RoleId_ActionId 
ON dbo.SysRoleAction (RoleId ASC, ActionId ASC);

CREATE NONCLUSTERED INDEX IX_SysRoleAction_ActionId_RoleId 
ON dbo.SysRoleAction (ActionId ASC, RoleId ASC);

/*SysDict*/
CREATE UNIQUE NONCLUSTERED INDEX IX_SysDict_Code 
ON dbo.SysDict (Code ASC)

CREATE NONCLUSTERED INDEX IX_SysDict_SortCode 
ON dbo.SysDict (SortCode ASC);

/*SysDictDetail*/
CREATE NONCLUSTERED INDEX IX_SysDictDetail_TypeId_SortCode 
ON dbo.SysDictDetail (DictId ASC, SortCode ASC)
INCLUDE (Name, Code);

CREATE UNIQUE NONCLUSTERED INDEX IX_SysDictDetail_TypeId_Code 
ON dbo.SysDictDetail (DictId ASC, Code ASC);

SELECT * FROM dbo.SysLogOperate