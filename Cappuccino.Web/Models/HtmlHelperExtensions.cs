using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Cappuccino.Model;

namespace Cappuccino.Web
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// 查询按钮
        /// </summary>
        public static HtmlString SearchBtnHtml(this HtmlHelper helper, string title = "查询", string _class = "pear-btn-primary")
        {
            return new HtmlString(string.Format(@"<button class='pear-btn pear-btn-md {1}' lay-submit lay-filter='search'>
                                                   <i class='layui-icon layui-icon-search'></i>{0}
                                                </button>", title, _class));
        }
        /// <summary>
        /// 重置按钮
        /// </summary>
        public static HtmlString ResetBtnHtml(this HtmlHelper helper, string title = "重置", string _class = "")
        {
            return new HtmlString(string.Format(@"<button type='reset' class='pear-btn pear-btn-md {1}'><i class='layui-icon layui-icon-refresh'></i>{0}</button>", title, _class));
        }

        /// <summary>
        /// 表格内按钮组（支持排除指定按钮）
        /// </summary>
        public static HtmlString RightToolBarHtml(this HtmlHelper helper, dynamic _list = null, params string[] excludeButtonCodes)
        {
            StringBuilder sb = new StringBuilder();
            List<ButtonModel> list = _list as List<ButtonModel>;
            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    // 判断当前按钮是否在排除列表中，不在则添加
                    if (excludeButtonCodes == null || !excludeButtonCodes.Contains(item.ButtonCode))
                    {
                        sb.AppendLine(string.Format(@"<button class='{0}' lay-event='{1}'><i class='layui-icon {2}'></i></button>", item.ClassName, item.ButtonCode, item.Icon));
                    }
                }
            }
            return new HtmlString(sb.ToString());
        }

        /// <summary>
        /// 表格外按钮组
        /// </summary>
        public static HtmlString TopToolBarHtml(this HtmlHelper helper, dynamic _list = null)
        {
            StringBuilder sb = new StringBuilder();
            List<ButtonModel> list = _list as List<ButtonModel>;
            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    sb.AppendLine(string.Format(@"<button class='{0}' lay-event='{1}'><i class='layui-icon {3}'></i>{2}</button>", item.ClassName, item.ButtonCode, item.FullName, item.Icon));
                }
            }
            return new HtmlString(sb.ToString());
        }

        /// <summary>
        /// 状态单选框
        /// </summary>
        public static HtmlString EnabledMarkRadioHtml(this HtmlHelper helper, string keyName = "enabledMark", int defaultVal = 0)
        {
            var enabled = defaultVal == 1 ? "checked" : "";
            var disabled = defaultVal == 0 ? "checked" : "";
            return new HtmlString(string.Format(@"<div class='layui-form-item'>
                                        <label class='layui-form-label'>状态</label>
                                        <div class='layui-input-block'>
                                            <input type='radio' name='{2}' value='1' title='启用' {0}>
                                            <input type='radio' name='{2}' value='0' title='禁用' {1}>
                                        </div>
                                    </div>", enabled, disabled, keyName));
        }
    }

    public static class HtmlHelperEnumExtensions
    {
        /// <summary>
        /// 扩展EnumDropDownListFor，支持指定默认选中的枚举值
        /// </summary>
        /// <typeparam name="TModel">模型类型</typeparam>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="htmlHelper">HtmlHelper实例</param>
        /// <param name="expression">标识要显示值的表达式</param>
        /// <param name="defaultValue">默认选中的枚举值</param>
        /// <returns>HTML select元素</returns>
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, TEnum defaultValue)
        {
            return htmlHelper.EnumDropDownListFor(expression, defaultValue, optionLabel: null);
        }

        /// <summary>
        /// 扩展EnumDropDownListFor，支持指定默认选中的枚举值和首选项文本
        /// </summary>
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, TEnum defaultValue, string optionLabel)
        {
            return htmlHelper.EnumDropDownListFor(expression, defaultValue, optionLabel, htmlAttributes: null);
        }

        /// <summary>
        /// 扩展EnumDropDownListFor，支持指定默认选中的枚举值、首选项文本和HTML属性（匿名对象形式）
        /// </summary>
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, TEnum defaultValue, string optionLabel, object htmlAttributes)
        {
            return htmlHelper.EnumDropDownListFor(expression, defaultValue, optionLabel, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// 扩展EnumDropDownListFor，支持指定默认选中的枚举值、首选项文本和HTML属性（字典形式）
        /// </summary>    
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, TEnum defaultValue, IDictionary<string, object> htmlAttributes)
        {
            return htmlHelper.EnumDropDownListFor(expression, defaultValue, optionLabel: null, htmlAttributes);
        }


        /// <summary>
        /// 核心实现：支持指定默认选中的枚举值、首选项文本和HTML属性字典
        /// </summary>
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, TEnum defaultValue, string optionLabel, IDictionary<string, object> htmlAttributes)
        {
            // 1. 验证枚举类型
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("TEnum必须是枚举类型", nameof(TEnum));

            // 2. 获取枚举所有值并转换为SelectListItem
            var enumValues = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
            var selectItems = new List<SelectListItem>();

            // 添加首选项（如"请选择"）
            if (!string.IsNullOrEmpty(optionLabel))
            {
                selectItems.Add(new SelectListItem
                {
                    Text = optionLabel,
                    Value = "",
                    Selected = false
                });
            }

            // 遍历枚举值，生成选项
            foreach (var enumValue in enumValues)
            {
                // 获取枚举成员元数据
                var memberInfo = typeof(TEnum).GetMember(enumValue.ToString()).FirstOrDefault();
                if (memberInfo == null)
                {
                    // 无元数据时使用枚举名
                    selectItems.Add(new SelectListItem
                    {
                        Text = enumValue.ToString(),
                        Value = ((int)(object)enumValue).ToString(), // Value使用整数
                        Selected = enumValue.Equals(defaultValue)
                    });
                    continue;
                }

                // 读取Display特性的中文名称（优先使用Display.Name）
                var displayAttr = memberInfo.GetCustomAttribute<DisplayAttribute>();
                string displayName = displayAttr?.GetName() ?? enumValue.ToString();

                selectItems.Add(new SelectListItem
                {
                    Text = displayName,       // 显示中文名称
                    Value = ((int)(object)enumValue).ToString(), // Value为枚举对应的整数
                    Selected = enumValue.Equals(defaultValue)    // 选中默认值
                });
            }

            // 3. 调用原生DropDownListFor生成HTML（复用模型绑定逻辑）
            return htmlHelper.DropDownListFor(expression, selectItems, optionLabel, htmlAttributes);
        }
    }
}
