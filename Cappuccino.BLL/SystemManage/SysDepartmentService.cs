using System.Collections.Generic;
using System.Linq;
using Cappuccino.Common.Extensions;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;
using Cappuccino.Model;

namespace Cappuccino.BLL
{
    public class SysDepartmentService : BaseService<SysDepartmentEntity>, ISysDepartmentService
    {
        #region 依赖注入
        private ISysDepartmentDao _departmentDao;
        private ISysUserDao _userDao;
        private ISysDataAuthorizeDao _dataAuthorizeDao;

        public SysDepartmentService(ISysDepartmentDao departmentDao, ISysUserDao userDao, ISysDataAuthorizeDao authorizeDao)
        {
            this._departmentDao = departmentDao;
            this._userDao = userDao;
            _dataAuthorizeDao = authorizeDao;
            base.CurrentDao = departmentDao;
            this.AddDisposableObject(this.CurrentDao);
        }
        #endregion

        /// <summary>
        /// 获取部门树
        /// </summary>
        /// <returns></returns>
        public List<DtreeData> GetDepartmentTree()
        {
            var sysDepartments = _departmentDao.GetList(x => true).ToList();
            //节点的名称为部门管理
            DtreeData node = new DtreeData
            {
                Title = "部门管理",
                Id = null,
                //定义树节点的子节点
                Children = new List<DtreeData>()
            };
            //遍历info中的list，目的是找到list中的父对象。list储存所有的对象（包括父对象，子对象，子对象的子对象等等）
            for (var i = 0; i < sysDepartments.Count(); i++)
            {
                SysDepartmentEntity departmentEntity = sysDepartments[i];
                //当对象的父类id为0的时候，说明这个是一个父对象
                if (departmentEntity.ParentId == 0)
                {
                    //定义一个新的easyUI树节点
                    DtreeData c = new DtreeData();
                    //节点的id就是这个父对象的id
                    c.Id = departmentEntity.Id.ToString();
                    //节点的名称就是这个父对象的名称
                    c.Title = departmentEntity.Name;
                    //获取父对象下的子对象
                    GetMenuChild(c, departmentEntity, sysDepartments);
                    //将这个父对象放入node之中（node相当于爷爷对象0_0）
                    node.Children.Add(c);
                }
            }
            List<DtreeData> list = new List<DtreeData>
            {
                node
            };
            return list;
        }

        /// <summary>
        /// 获取最大的排序号
        /// </summary>
        /// <returns></returns>
        public int GetMaxSortCode()
        {
            var result = _departmentDao.ExecuteSqlQuery<int?>("SELECT MAX(SortCode) FROM SysDepartment").FirstOrDefault();
            int maxSortCode = result.ParseToInt();
            maxSortCode++;
            return maxSortCode;
        }

        public List<DtreeData> GetDepartmentDtree(int roleId)
        {
            // 获取所有部门
            List<SysDepartmentEntity> departmentList = _departmentDao.GetList(x => true).ToList();
            // 获取所有用户
            List<SysUserEntity> userList = _userDao.GetList(x => true).ToList();

            List<DtreeData> allNodes = new List<DtreeData>();

            // 遍历部门，创建部门节点和用户子节点
            foreach (SysDepartmentEntity dept in departmentList)
            {
                // 创建部门节点
                DtreeData deptNode = new DtreeData();
                deptNode.Id = dept.Id.ToString();
                deptNode.Title = dept.Name;
                deptNode.Type = "dept";
                deptNode.ParentId = dept.ParentId.ToString();

                SysDataAuthorizeEntity deptDataAuthorize = _dataAuthorizeDao.GetList(x => x.DataId == dept.Id && x.AuthorizeType == 1 && x.AuthorizeId == roleId).FirstOrDefault();
                if (deptDataAuthorize != null)
                {
                    deptNode.CheckArr = "1";
                }
                allNodes.Add(deptNode);

                // 添加部门下的用户作为子节点
                List<SysUserEntity> deptUsersList = userList.Where(u => u.DepartmentId == dept.Id).ToList();
                foreach (SysUserEntity user in deptUsersList)
                {
                    DtreeData userNode = new DtreeData();
                    userNode.Id = user.Id.ToString();
                    userNode.Title = user.UserName;
                    userNode.Type = "user";
                    userNode.ParentId = dept.Id.ToString();

                    SysDataAuthorizeEntity userDataAuthorize = _dataAuthorizeDao.GetList(x => x.DataId == user.Id && x.AuthorizeType == 1 && x.AuthorizeId == roleId).FirstOrDefault();
                    if (userDataAuthorize != null)
                    {
                        userNode.CheckArr = "1";
                    }
                    allNodes.Add(userNode);
                }
            }

            // 构建树形结构数据
            List<DtreeData> dtreeDatas = new List<DtreeData>();
            Dictionary<string, DtreeData> nodeDict = new Dictionary<string, DtreeData>();
            foreach (var item in allNodes)
            {
                nodeDict.Add(item.Id.ToString(), item);
                if (item.ParentId == "0")
                {
                    dtreeDatas.Add(item);
                }
            }

            foreach (var item in allNodes)
            {
                if (item.ParentId != "0")
                {
                    nodeDict[item.ParentId].Children.Add(item);
                }
            }

            return dtreeDatas;
        }

        public List<DtreeData> GetDepartmentDtree1(int roleId)
        {
            // 1. 批量查询基础数据（仅3次数据库查询，避免循环查库）
            List<SysDepartmentEntity> departmentList = _departmentDao.GetList(x => true).ToList();
            List<SysUserEntity> userList = _userDao.GetList(x => true).ToList();

            // 批量查询所有权限记录，缓存为字典（Key=数据ID，Value=是否有权限）
            var authorizeDict = _dataAuthorizeDao
                .GetList(x => x.AuthorizeType == 1 && x.AuthorizeId == roleId)
                .ToDictionary(key => key.DataId.ToString(), value => true);

            // 2. 构建所有部门节点（先不处理父子关系，确保所有部门都在字典中）
            Dictionary<string, DtreeData> deptNodeDict = new Dictionary<string, DtreeData>();
            foreach (var dept in departmentList)
            {
                var deptNode = new DtreeData
                {
                    Id = dept.Id.ToString(),
                    Title = dept.Name,
                    Type = "dept",
                    ParentId = dept.ParentId.ToString(),
                    Children = new List<DtreeData>(), // 初始化子节点集合（用于存放子部门和用户）
                    CheckArr = authorizeDict.ContainsKey(dept.Id.ToString()) ? "1" : "0" // 部门权限
                };
                deptNodeDict[deptNode.Id] = deptNode;
            }

            // 3. 绑定部门间的父子关系（构建部门层级）
            foreach (var deptNode in deptNodeDict.Values)
            {
                // 非根部门：找到父部门，加入其父节点的Children
                if (deptNode.ParentId != "0" && deptNodeDict.TryGetValue(deptNode.ParentId, out var parentDept))
                {
                    parentDept.Children.Add(deptNode);
                }
            }

            // 4. 处理用户节点（作为部门的子节点，加入对应部门的Children）
            foreach (var user in userList)
            {
                var deptId = user.DepartmentId.ToString();
                // 只处理存在的部门下的用户
                if (!deptNodeDict.TryGetValue(deptId, out var parentDept))
                    continue;

                var userNode = new DtreeData
                {
                    Id = user.Id.ToString(),
                    Title = user.UserName,
                    Type = "user",
                    ParentId = deptId,
                    Children = new List<DtreeData>(), // 用户无子集
                    CheckArr = authorizeDict.ContainsKey(user.Id.ToString()) ? "1" : "0" // 用户权限
                };
                parentDept.Children.Add(userNode); // 加入父部门的子节点
            }

            // 5. 筛选根节点（ParentId=0的部门）作为返回结果
            return deptNodeDict.Values.Where(dept => dept.ParentId == "0").ToList();
        }

        private void GetMenuChild(DtreeData parent, SysDepartmentEntity uparent, List<SysDepartmentEntity> allDepartment)
        {
            //遍历所有的对象
            for (var i = 0; i < allDepartment.Count; i++)
            {
                SysDepartmentEntity departmentEntity = allDepartment[i];
                //如果这个对象的父id和这个父对象的id是相同的，那么说明这个对象是父对象的子对象
                if (departmentEntity.ParentId == uparent.Id)
                {
                    //设置一个新的子对象，这里又用一个node来命名新的子对象不是很妥当
                    DtreeData node = new DtreeData();
                    //设置子对象的id
                    node.Id = departmentEntity.Id.ToString();
                    //设置子对象的名称
                    node.Title = departmentEntity.Name;
                    //一个递归调用，查找子对象是否还存在子对象
                    GetMenuChild(node, departmentEntity, allDepartment);
                    //将这个子对象放入父easyUI树中
                    parent.Children.Add(node);
                }
            }
        }
    }
}
