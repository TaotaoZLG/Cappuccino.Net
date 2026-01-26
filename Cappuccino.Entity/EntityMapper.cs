using AutoMapper;
using Cappuccino.Model;

namespace Cappuccino.Entity
{
    public static class EntityMapper
    {
        /// <summary>
        /// 负责将所有实体做一次映射操作，注册类型的映射关系
        /// </summary>
        static EntityMapper()
        {
            //1.将Entity和Model中的所有实体类在AutoMapper内部建立一个关联
            Mapper.CreateMap<SysUserEntity, SysUserModel>();
            Mapper.CreateMap<SysRoleEntity, SysRoleModel>();
            Mapper.CreateMap<SysActionEntity, SysActionModel>();
            Mapper.CreateMap<SysActionMenuEntity, SysActionMenuModel>();
            Mapper.CreateMap<SysActionButtonEntity, SysActionButtonModel>();
            Mapper.CreateMap<SysUserActionEntity, SysUserActionModel>();
            Mapper.CreateMap<SysDictDetailEntity, SysDictDetailModel>();
            Mapper.CreateMap<SysDictEntity, SysDictModel>();
            Mapper.CreateMap<SysLogLogonEntity, SysLogLogonModel>();
            Mapper.CreateMap<SysAutoJobEntity, SysAutoJobModel>();
            Mapper.CreateMap<SysNoticeEntity, SysNoticeModel>();
            Mapper.CreateMap<SysConfigEntity, SysConfigModel>();

            //2.将Model和Entity中的所有实体类在AutoMapper内部建立一个关联
            Mapper.CreateMap<SysUserModel, SysUserEntity>();
            Mapper.CreateMap<SysRoleModel, SysRoleEntity>();
            Mapper.CreateMap<SysActionModel, SysActionEntity>();
            Mapper.CreateMap<SysActionMenuModel, SysActionMenuEntity>();
            Mapper.CreateMap<SysActionButtonModel, SysActionButtonEntity>();
            Mapper.CreateMap<SysUserActionModel, SysUserActionEntity>();
            Mapper.CreateMap<SysDictDetailModel, SysDictDetailEntity>();
            Mapper.CreateMap<SysDictModel, SysDictEntity>();
            Mapper.CreateMap<SysLogLogonModel, SysLogLogonEntity>();
            Mapper.CreateMap<SysAutoJobModel, SysAutoJobEntity>();
            Mapper.CreateMap<SysNoticeModel, SysNoticeEntity>();
            Mapper.CreateMap<SysConfigModel, SysConfigEntity>();
        }

        #region SysUser
        public static SysUserModel EntityMap(this SysUserEntity model)
        {
            return Mapper.Map<SysUserEntity, SysUserModel>(model);
        }

        public static SysUserEntity EntityMap(this SysUserModel model)
        {
            return Mapper.Map<SysUserModel, SysUserEntity>(model);
        }

        #endregion

        #region SysRole
        public static SysRoleModel EntityMap(this SysRoleEntity model)
        {
            return Mapper.Map<SysRoleEntity, SysRoleModel>(model);
        }

        public static SysRoleEntity EntityMap(this SysRoleModel model)
        {
            return Mapper.Map<SysRoleModel, SysRoleEntity>(model);
        }
        #endregion

        #region SysAction
        public static SysActionModel EntityMap(this SysActionEntity model)
        {
            return Mapper.Map<SysActionEntity, SysActionModel>(model);
        }

        public static SysActionEntity EntityMap(this SysActionModel model)
        {
            return Mapper.Map<SysActionModel, SysActionEntity>(model);
        }
        #endregion

        #region SysActionMenu
        public static SysActionMenuModel EntityMap(this SysActionMenuEntity model)
        {
            return Mapper.Map<SysActionMenuEntity, SysActionMenuModel>(model);
        }

        public static SysActionMenuEntity EntityMap(this SysActionMenuModel model)
        {
            return Mapper.Map<SysActionMenuModel, SysActionMenuEntity>(model);
        }
        #endregion

        #region SysActionButton
        public static SysActionButtonModel EntityMap(this SysActionButtonEntity model)
        {
            return Mapper.Map<SysActionButtonEntity, SysActionButtonModel>(model);
        }

        public static SysActionButtonEntity EntityMap(this SysActionButtonModel model)
        {
            return Mapper.Map<SysActionButtonModel, SysActionButtonEntity>(model);
        }
        #endregion

        #region SysUserAction
        public static SysUserActionModel EntityMap(this SysUserActionEntity model)
        {
            return Mapper.Map<SysUserActionEntity, SysUserActionModel>(model);
        }

        public static SysUserActionEntity EntityMap(this SysUserActionModel model)
        {
            return Mapper.Map<SysUserActionModel, SysUserActionEntity>(model);
        }
        #endregion

        #region SysDictDetail
        public static SysDictDetailModel EntityMap(this SysDictDetailEntity model)
        {
            return Mapper.Map<SysDictDetailEntity, SysDictDetailModel>(model);
        }

        public static SysDictDetailEntity EntityMap(this SysDictDetailModel model)
        {
            return Mapper.Map<SysDictDetailModel, SysDictDetailEntity>(model);
        }
        #endregion

        #region SysDict
        public static SysDictModel EntityMap(this SysDictEntity model)
        {
            return Mapper.Map<SysDictEntity, SysDictModel>(model);
        }

        public static SysDictEntity EntityMap(this SysDictModel model)
        {
            return Mapper.Map<SysDictModel, SysDictEntity>(model);
        }
        #endregion

        #region SysLogLogon
        public static SysLogLogonModel EntityMap(this SysLogLogonEntity model)
        {
            return Mapper.Map<SysLogLogonEntity, SysLogLogonModel>(model);
        }

        public static SysLogLogonEntity EntityMap(this SysLogLogonModel model)
        {
            return Mapper.Map<SysLogLogonModel, SysLogLogonEntity>(model);
        }
        #endregion

        #region SysAutoJob
        public static SysAutoJobModel EntityMap(this SysAutoJobEntity model)
        {
            return Mapper.Map<SysAutoJobEntity, SysAutoJobModel>(model);
        }

        public static SysAutoJobEntity EntityMap(this SysAutoJobModel model)
        {
            return Mapper.Map<SysAutoJobModel, SysAutoJobEntity>(model);
        }
        #endregion

        #region SysNotice
        public static SysNoticeModel EntityMap(this SysNoticeEntity model)
        {
            return Mapper.Map<SysNoticeEntity, SysNoticeModel>(model);
        }

        public static SysNoticeEntity EntityMap(this SysNoticeModel model)
        {
            return Mapper.Map<SysNoticeModel, SysNoticeEntity>(model);
        }
        #endregion

        #region SysConfig
        public static SysConfigModel EntityMap(this SysConfigEntity model)
        {
            return Mapper.Map<SysConfigEntity, SysConfigModel>(model);
        }

        public static SysConfigEntity EntityMap(this SysConfigModel model)
        {
            return Mapper.Map<SysConfigModel, SysConfigEntity>(model);
        }
        #endregion
    }
}
