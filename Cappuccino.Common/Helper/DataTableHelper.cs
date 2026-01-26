using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Cappuccino.Common.Helper
{
    public class DataTableHelper
    {
        /// <summary>
        /// List转DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public static DataTable ListToDataTable<T>(List<T> entitys)
        {
            //检查实体集合不能为空
            if (entitys == null || entitys.Count < 1)
            {
                throw new Exception("需转换的集合为空");
            }
            //取出第一个实体的所有Propertie
            Type entityType = entitys[0].GetType();
            PropertyInfo[] entityProperties = entityType.GetProperties();

            //生成DataTable的structure
            //生产代码中，应将生成的DataTable结构Cache起来，此处略
            DataTable dt = new DataTable();
            for (int i = 0; i < entityProperties.Length; i++)
            {
                dt.Columns.Add(entityProperties[i].Name);
            }
            //将所有entity添加到DataTable中
            foreach (object entity in entitys)
            {
                //检查所有的的实体都为同一类型
                if (entity.GetType() != entityType)
                {
                    throw new Exception("要转换的集合元素类型不一致");
                }
                object[] entityValues = new object[entityProperties.Length];
                for (int i = 0; i < entityProperties.Length; i++)
                {
                    entityValues[i] = entityProperties[i].GetValue(entity, null);
                }
                dt.Rows.Add(entityValues);
            }
            return dt;
        }

        /// <summary>
        /// DataTable转List
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> DataTableToList(DataTable dt)
        {

            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            foreach (DataRow dr in dt.Rows)//每一行信息，新建一个Dictionary<string,object>,将该行的每列信息加入到字典
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                foreach (DataColumn dc in dt.Columns)
                {
                    result.Add(dc.ColumnName, dr[dc].ToString());
                }
                list.Add(result);
            }
            return list;
        }

        /// <summary>
        /// Dataset转List
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static List<List<Dictionary<string, object>>> DataSetToList(DataSet dataSet)
        {
            List<List<Dictionary<string, object>>> result = new List<List<Dictionary<string, object>>>();

            foreach (DataTable dataTable in dataSet.Tables)
            {
                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                foreach (DataRow dr in dataTable.Rows)
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    foreach (DataColumn dc in dataTable.Columns)
                    {
                        row.Add(dc.ColumnName, dr[dc]);
                    }
                    list.Add(row);
                }
                result.Add(list);
            }

            return result;
        }
    }
}
