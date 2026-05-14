using System.Collections.Generic;
using System.Linq;
using Cappuccino.Cache;
using Cappuccino.Common.Extensions;
using Cappuccino.Entity;
using Cappuccino.IBLL;
using Cappuccino.IDAL;
using Cappuccino.Model;

namespace Cappuccino.BLL
{
    public class SysDictService : BaseService<SysDictEntity>, ISysDictService
    {
        private readonly ISysDictDao _dictDao;
        private readonly ISysDictDetailDao _sysDictDetailDao;

        private DataDictCache _dataDictCache;
        private DataDictDetailCache _dataDictDetailCache;

        #region 依赖注入
        public SysDictService(ISysDictDao dictDao, ISysDictDetailDao sysDictDetailDao)
        {
            _dictDao = dictDao;
            _sysDictDetailDao = sysDictDetailDao;
            _dataDictCache = new DataDictCache(_dictDao);
            _dataDictDetailCache = new DataDictDetailCache(_sysDictDetailDao);
            base.CurrentDao = dictDao;
            this.AddDisposableObject(this.CurrentDao);
            
        }
        #endregion

        /// <summary>
        /// 获取最大的排序号
        /// </summary>
        /// <returns></returns>
        public int GetMaxSortCode()
        {
            var result = _dictDao.ExecuteSqlQuery<int?>($"SELECT MAX(SortCode) FROM SysDict WHERE 1=1").FirstOrDefault();
            int maxSortCode = result.ParseToInt();
            maxSortCode++;
            return maxSortCode;
        }

        public List<DataDictInfo> GetDataDictList()
        {
            List<SysDictEntity> dataDictList =  _dataDictCache.GetList();
            List<SysDictDetailEntity> dataDictDetailList = _dataDictDetailCache.GetList();

            List<DataDictInfo> dataDictInfoList = new List<DataDictInfo>();

            dataDictInfoList = dataDictList.Select(type => new DataDictInfo
            {
                DictCode = type.Code,  // 字典类型编码
                DictName = type.Name,  // 字典类型名称
                DictInfo = dataDictDetailList.Where(d => d.DictId == type.Id)
                    .Select(d => new DataDictDetailInfo
                    {
                        Name = d.Name,  // 字典项名称
                        Value = d.Code,  // 字典项值
                        Sort = d.SortCode,  // 排序号
                        Class = d.ListClass  // 显示样式
                    })
                    .OrderBy(d => d.Sort)
                    .ToList()
            }).ToList();

            return dataDictInfoList;
        }
    }
}
