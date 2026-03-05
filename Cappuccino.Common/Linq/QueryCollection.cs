using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cappuccino.Common.Extensions;

namespace Cappuccino.Common
{
    public class Query
    {
        public enum Operators
        {
            /// <summary>
            /// 空
            /// </summary>
            None = 0,
            /// <summary>
            /// 等于
            /// </summary>
            Equal = 1,
            /// <summary>
            ///  大于 >
            /// </summary>
            GreaterThan = 2,
            /// <summary>
            /// 大于等于 >=
            /// </summary>
            GreaterThanOrEqual = 3,
            /// <summary>
            /// 小于 <
            /// </summary>
            LessThan = 4,
            /// <summary>
            /// 小于 <=
            /// </summary>
            LessThanOrEqual = 5,
            /// <summary>
            /// Contains 等价于 like '%key%
            /// </summary>
            Contains = 6,
            /// <summary>
            /// StartsWith等价于like 'key%'
            /// </summary>
            StartWith = 7,
            /// <summary>
            /// EndsWith等价于like '%key'
            /// </summary>
            EndWidth = 8,
            /// <summary>
            /// 范围
            /// </summary>
            Range = 9,
            /// <summary>
            /// 包含在集合中（等价于 IN）
            /// </summary>
            In = 10
        }
        public enum Condition
        {
            /// <summary>
            /// 或（OR）
            /// </summary>
            OrElse = 1,
            /// <summary>
            /// 且（AND）
            /// </summary>
            AndAlso = 2
        }
        public string Name { get; set; }
        public Operators Operator { get; set; }
        public object Value { get; set; }
        public object ValueMin { get; set; }
        public object ValueMax { get; set; }
    }

    /// <summary>
    /// 动态构建Lambda表达式实现EF动态查询
    /// </summary>
    public class QueryCollection : Collection<Query>
    {
        public Expression<Func<T, bool>> AsExpression<T>(Query.Condition? condition = Query.Condition.OrElse) where T : class
        {
            Type targetType = typeof(T);
            TypeInfo typeInfo = targetType.GetTypeInfo();
            var parameter = Expression.Parameter(targetType, "m");
            Expression expression = null;
            Func<Expression, Expression, Expression> Append = (exp1, exp2) =>
            {
                if (exp1 == null)
                {
                    return exp2;
                }
                // 默认用 OrElse（逻辑或）连接条件
                return (condition ?? Query.Condition.OrElse) == Query.Condition.OrElse ? Expression.OrElse(exp1, exp2) : Expression.AndAlso(exp1, exp2);
            };
            foreach (var item in this)
            {
                var property = typeInfo.GetProperty(item.Name);
                if (property == null ||
                    !property.CanRead ||
                    (item.Operator != Query.Operators.Range && item.Value == null) ||
                    (item.Operator == Query.Operators.Range && item.ValueMin == null && item.ValueMax == null))
                {
                    continue;
                }
                Type realType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (item.Operator != Query.Operators.In && item.Value != null)
                {
                    item.Value = Convert.ChangeType(item.Value, realType);
                }
                Expression<Func<object>> valueLamba = () => item.Value;
                switch (item.Operator)
                {
                    case Query.Operators.Equal:
                        {
                            expression = Append(expression, Expression.Equal(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Query.Operators.GreaterThan:
                        {
                            expression = Append(expression, Expression.GreaterThan(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Query.Operators.GreaterThanOrEqual:
                        {
                            expression = Append(expression, Expression.GreaterThanOrEqual(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Query.Operators.LessThan:
                        {
                            expression = Append(expression, Expression.LessThan(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Query.Operators.LessThanOrEqual:
                        {
                            expression = Append(expression, Expression.LessThanOrEqual(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Query.Operators.Contains:
                        {
                            var nullCheck = Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, Expression.Property(parameter, item.Name)));
                            var contains = Expression.Call(Expression.Property(parameter, item.Name), "Contains", null,
                                Expression.Convert(valueLamba.Body, property.PropertyType));
                            expression = Append(expression, Expression.AndAlso(nullCheck, contains));
                            break;
                        }
                    case Query.Operators.StartWith:
                        {
                            var nullCheck = Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, Expression.Property(parameter, item.Name)));
                            var startsWith = Expression.Call(Expression.Property(parameter, item.Name), "StartsWith", null,
                                Expression.Convert(valueLamba.Body, property.PropertyType));
                            expression = Append(expression, Expression.AndAlso(nullCheck, startsWith));
                            break;
                        }
                    case Query.Operators.EndWidth:
                        {
                            var nullCheck = Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, Expression.Property(parameter, item.Name)));
                            var endsWith = Expression.Call(Expression.Property(parameter, item.Name), "EndsWith", null,
                                Expression.Convert(valueLamba.Body, property.PropertyType));
                            expression = Append(expression, Expression.AndAlso(nullCheck, endsWith));
                            break;
                        }
                    case Query.Operators.Range:
                        {
                            Expression minExp = null, maxExp = null;
                            if (item.ValueMin != null)
                            {
                                var minValue = Convert.ChangeType(item.ValueMin, realType);
                                Expression<Func<object>> minValueLamda = () => minValue;
                                minExp = Expression.GreaterThanOrEqual(Expression.Property(parameter, item.Name), Expression.Convert(minValueLamda.Body, property.PropertyType));
                            }
                            if (item.ValueMax != null)
                            {
                                var maxValue = Convert.ChangeType(item.ValueMax, realType);
                                Expression<Func<object>> maxValueLamda = () => maxValue;
                                maxExp = Expression.LessThanOrEqual(Expression.Property(parameter, item.Name), Expression.Convert(maxValueLamda.Body, property.PropertyType));
                            }

                            if (minExp != null && maxExp != null)
                            {
                                expression = Append(expression, Expression.AndAlso(minExp, maxExp));
                            }
                            else if (minExp != null)
                            {
                                expression = Append(expression, minExp);
                            }
                            else if (maxExp != null)
                            {
                                expression = Append(expression, maxExp);
                            }

                            break;
                        }
                    case Query.Operators.In:
                        {
                            // 校验Value是否为string数组（或IEnumerable<string>）
                            if (item.Value == null)
                                break;

                            // 1. 统一转换为IEnumerable<object>（兼容string[]/int[]/List<int>/List<string>等）
                            IEnumerable<object> valueEnumerable = null;
                            // 处理数组（任意类型数组）
                            if (item.Value is Array array)
                            {
                                valueEnumerable = array.Cast<object>();
                            }
                            // 处理IEnumerable（List/HashSet等）
                            else if (item.Value is IEnumerable<object> enumerable)
                            {
                                valueEnumerable = enumerable;
                            }
                            // 处理字符串（逗号分隔的ID字符串，兼容前端传参）
                            else if (item.Value is string str && str.Contains(","))
                            {
                                valueEnumerable = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(s => (object)s);
                            }
                            // 处理单个值（前端只选1条数据的情况）
                            else
                            {
                                valueEnumerable = new List<object> { item.Value };
                            }

                            if (valueEnumerable == null || !valueEnumerable.Any())
                                break;

                            // 2. 转换集合元素类型与实体属性一致
                            var convertedValues = new List<object>();
                            foreach (var val in valueEnumerable)
                            {
                                if (val == null || string.IsNullOrWhiteSpace(val.ToString()))
                                    continue;

                                object convertedVal;
                                try
                                {
                                    // 优先用项目扩展方法转换（兼容自定义转换逻辑）
                                    if (realType == typeof(int))
                                        convertedVal = val.ToString().ParseToInt();
                                    else if (realType == typeof(long))
                                        convertedVal = val.ToString().ParseToLong();
                                    else if (realType == typeof(Guid))
                                        convertedVal = val.ToString().ParseToGuid();
                                    else if (realType == typeof(DateTime))
                                        convertedVal = val.ToString().ParseToDateTime();
                                    else if (realType == typeof(string))
                                        convertedVal = val.ToString().Trim();
                                    else
                                        convertedVal = Convert.ChangeType(val, realType);
                                }
                                catch
                                {
                                    continue; // 转换失败的元素跳过，不影响整体查询
                                }
                                convertedValues.Add(convertedVal);
                            }

                            if (!convertedValues.Any())
                                break;

                            // 3. 构建强类型数组（解决Contains方法泛型类型不匹配问题）
                            var convertedArray = Array.CreateInstance(realType, convertedValues.Count);
                            for (int i = 0; i < convertedValues.Count; i++)
                            {
                                convertedArray.SetValue(convertedValues[i], i);
                            }
                            var arrayExpr = Expression.Constant(convertedArray);

                            // 4. 构建属性访问表达式
                            var propertyAccess = Expression.Property(parameter, item.Name);

                            // 5. 精准获取Contains方法（避免重载匹配错误）
                            var containsMethod = typeof(Enumerable)
                                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                                .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                                .MakeGenericMethod(realType);

                            // 6. 构建Contains表达式
                            var containsExpr = Expression.Call(containsMethod, arrayExpr, propertyAccess);

                            // 7. 添加到整体表达式
                            expression = Append(expression, containsExpr);
                            break;
                        }
                }
            }
            if (expression == null)
            {
                return x => true;
            }
            return ((Expression<Func<T, bool>>)Expression.Lambda(expression, parameter));
        }
    }
}
