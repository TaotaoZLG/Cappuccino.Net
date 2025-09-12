using AutoMapper;
using Cappuccino.Entity;

namespace Cappuccino.Model
{
    public static class EntityMapper
    {
        /// <summary>
        /// 负责将所有实体做一次映射操作
        /// </summary>
        static EntityMapper()
        {
            //1.将Model和Model中的所有实体类在AutoMapper内部建立一个关联
            Mapper.CreateMap<SysUserEntity, SysUserViewModel>();
            Mapper.CreateMap<SysRoleEntity, SysRoleViewModel>();
            Mapper.CreateMap<SysActionEntity, SysActionViewModel>();
            Mapper.CreateMap<SysActionMenuEntity, SysActionMenuViewModel>();
            Mapper.CreateMap<SysActionButtonEntity, SysActionButtonViewModel>();
            Mapper.CreateMap<SysUserActionEntity, SysUserActionViewModel>();
            Mapper.CreateMap<SysDictDetailEntity, SysDictDetailViewModel>();
            Mapper.CreateMap<SysDictEntity, SysDictViewModel>();
            Mapper.CreateMap<SysLogLogonEntity, SysLogLogonViewModel>();

            //2.将Model和Model中的所有实体类在AutoMapper内部建立一个关联
            Mapper.CreateMap<SysUserViewModel, SysUserEntity>();
            Mapper.CreateMap<SysRoleViewModel, SysRoleEntity>();
            Mapper.CreateMap<SysActionViewModel, SysActionEntity>();
            Mapper.CreateMap<SysActionMenuViewModel, SysActionMenuEntity>();
            Mapper.CreateMap<SysActionButtonViewModel, SysActionButtonEntity>();
            Mapper.CreateMap<SysUserActionViewModel, SysUserActionEntity>();
            Mapper.CreateMap<SysDictDetailViewModel, SysDictDetailEntity>();
            Mapper.CreateMap<SysDictViewModel, SysDictEntity>();
            Mapper.CreateMap<SysLogLogonViewModel, SysLogLogonEntity>();
        }

        #region SysUser
        public static SysUserViewModel EntityMap(this SysUserEntity model)
        {
            return Mapper.Map<SysUserEntity, SysUserViewModel>(model);
        }

        public static SysUserEntity EntityMap(this SysUserViewModel model)
        {
            return Mapper.Map<SysUserViewModel, SysUserEntity>(model);
        }

        #endregion

        #region SysRole
        public static SysRoleViewModel EntityMap(this SysRoleEntity model)
        {
            return Mapper.Map<SysRoleEntity, SysRoleViewModel>(model);
        }

        public static SysRoleEntity EntityMap(this SysRoleViewModel model)
        {
            return Mapper.Map<SysRoleViewModel, SysRoleEntity>(model);
        }
        #endregion

        #region SysAction
        public static SysActionViewModel EntityMap(this SysActionEntity model)
        {
            return Mapper.Map<SysActionEntity, SysActionViewModel>(model);
        }

        public static SysActionEntity EntityMap(this SysActionViewModel model)
        {
            return Mapper.Map<SysActionViewModel, SysActionEntity>(model);
        }
        #endregion

        #region SysActionMenu
        public static SysActionMenuViewModel EntityMap(this SysActionMenuEntity model)
        {
            return Mapper.Map<SysActionMenuEntity, SysActionMenuViewModel>(model);
        }

        public static SysActionMenuEntity EntityMap(this SysActionMenuViewModel model)
        {
            return Mapper.Map<SysActionMenuViewModel, SysActionMenuEntity>(model);
        }
        #endregion

        #region SysActionButton
        public static SysActionButtonViewModel EntityMap(this SysActionButtonEntity model)
        {
            return Mapper.Map<SysActionButtonEntity, SysActionButtonViewModel>(model);
        }

        public static SysActionButtonEntity EntityMap(this SysActionButtonViewModel model)
        {
            return Mapper.Map<SysActionButtonViewModel, SysActionButtonEntity>(model);
        }
        #endregion

        #region SysUserAction
        public static SysUserActionViewModel EntityMap(this SysUserActionEntity model)
        {
            return Mapper.Map<SysUserActionEntity, SysUserActionViewModel>(model);
        }

        public static SysUserActionEntity EntityMap(this SysUserActionViewModel model)
        {
            return Mapper.Map<SysUserActionViewModel, SysUserActionEntity>(model);
        }
        #endregion

        #region SysDictDetail
        public static SysDictDetailViewModel EntityMap(this SysDictDetailEntity model)
        {
            return Mapper.Map<SysDictDetailEntity, SysDictDetailViewModel>(model);
        }

        public static SysDictDetailEntity EntityMap(this SysDictDetailViewModel model)
        {
            return Mapper.Map<SysDictDetailViewModel, SysDictDetailEntity>(model);
        }
        #endregion

        #region SysDict
        public static SysDictViewModel EntityMap(this SysDictEntity model)
        {
            return Mapper.Map<SysDictEntity, SysDictViewModel>(model);
        }

        public static SysDictEntity EntityMap(this SysDictViewModel model)
        {
            return Mapper.Map<SysDictViewModel, SysDictEntity>(model);
        }
        #endregion

        #region SysLogLogon
        public static SysLogLogonViewModel EntityMap(this SysLogLogonEntity model)
        {
            return Mapper.Map<SysLogLogonEntity, SysLogLogonViewModel>(model);
        }

        public static SysLogLogonEntity EntityMap(this SysLogLogonViewModel model)
        {
            return Mapper.Map<SysLogLogonViewModel, SysLogLogonEntity>(model);
        }
        #endregion
    }
}
