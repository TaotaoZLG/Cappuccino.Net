using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using NPOI.XWPF.UserModel;

namespace Cappuccino.Common.Helpers
{
    /// <summary>
    /// Word文档域值替换帮助类（基于实体DisplayName注解）
    /// 支持 NPOI 2.6.1+
    /// </summary>
    public static class WordHelper
    {
        /// <summary>
        /// 批量替换Word文档中的占位符 {占位符名称}
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="wordFilePath">Word文件物理路径</param>
        /// <param name="entity">数据实体</param>
        /// <param name="dateFormat">日期格式化，默认 yyyy-MM-dd</param>
        public static void ReplaceContent<T>(string wordFilePath, T entity, string dateFormat = "yyyy-MM-dd") where T : class
        {
            // 构建映射字典
            var keyValues = BuildPlaceholderMap(entity, dateFormat);

            using (var fs = new FileStream(wordFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                XWPFDocument doc = new XWPFDocument(fs);
                // 替换正文段落
                ReplaceParagraphs(doc.Paragraphs, keyValues);
                // 替换表格
                ReplaceTables(doc.Tables, keyValues);
                // 替换页眉和页脚
                ReplaceHeadersFooters(doc, keyValues);

                // 保存覆盖原文件
                // 1. 将流指针移回开头
                fs.Seek(0, SeekOrigin.Begin);
                // 2. 截断流（清空旧内容），防止新内容比旧内容短时残留垃圾数据
                fs.SetLength(0);

                // 3. 直接写入当前流
                doc.Write(fs);

                // 4. 强制刷新缓冲区
                fs.Flush();
            }
        }

        #region 私有核心方法

        /// <summary>
        /// 构建实体属性 => {DisplayName} 映射字典
        /// </summary>
        private static Dictionary<string, string> BuildPlaceholderMap<T>(T entity, string dateFormat)
        {
            var map = new Dictionary<string, string>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                // 获取 DisplayName 特性
                var displayAttr = prop.GetCustomAttribute<DisplayNameAttribute>();
                if (displayAttr == null || string.IsNullOrWhiteSpace(displayAttr.DisplayName))
                    continue;

                // 获取值
                object value = prop.GetValue(entity);
                string strValue = string.Empty;

                if (value != null)
                {
                    // 日期特殊处理
                    if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                    {
                        strValue = ((DateTime)value).ToString(dateFormat);
                    }
                    else
                    {
                        strValue = value.ToString();
                    }
                }

                // 构造 Key: {客户姓名}
                string key = $"{{{displayAttr.DisplayName}}}";

                // 避免重复Key报错，如果实体中有重复DisplayName，后一个覆盖前一个
                if (map.ContainsKey(key))
                    map[key] = strValue;
                else
                    map.Add(key, strValue);
            }
            return map;
        }

        /// <summary>
        /// 替换段落列表
        /// </summary>
        private static void ReplaceParagraphs(IList<XWPFParagraph> paragraphs, Dictionary<string, string> map)
        {
            if (paragraphs == null) return;
            foreach (var para in paragraphs)
            {
                ReplaceSingleParagraph(para, map);
            }
        }

        /// <summary>
        /// 替换表格内的段落
        /// </summary>
        private static void ReplaceTables(IList<XWPFTable> tables, Dictionary<string, string> map)
        {
            if (tables == null) return;
            foreach (var table in tables)
            {
                foreach (var row in table.Rows)
                {
                    foreach (var cell in row.GetTableCells())
                    {
                        ReplaceParagraphs(cell.Paragraphs, map);
                    }
                }
            }
        }

        /// <summary>
        /// 【新增】替换页眉和页脚
        /// </summary>
        private static void ReplaceHeadersFooters(XWPFDocument doc, Dictionary<string, string> map)
        {
            // 替换页眉
            if (doc.HeaderList != null)
            {
                foreach (var header in doc.HeaderList)
                {
                    ReplaceParagraphs(header.Paragraphs, map);
                }
            }
            // 替换页脚
            if (doc.FooterList != null)
            {
                foreach (var footer in doc.FooterList)
                {
                    ReplaceParagraphs(footer.Paragraphs, map);
                }
            }
        }

        /// <summary>
        /// 替换单个段落
        /// </summary>
        private static void ReplaceSingleParagraph(XWPFParagraph para, Dictionary<string, string> map)
        {
            if (para == null) return;

            // 1. 快速检查：段落文本是否包含任何占位符？
            // 使用 ParagraphText 属性获取拼接后的全文本
            string fullText = para.ParagraphText;
            if (string.IsNullOrWhiteSpace(fullText)) return;

            bool hasPlaceholder = map.Keys.Any(k => fullText.Contains(k));
            if (!hasPlaceholder) return;

            // 2. 尝试策略 A：原地替换 (保留完美格式)
            // 遍历所有 Run，如果占位符完整存在于某个 Run 中，直接替换
            var runs = para.Runs; // ✅ 正确属性名

            if (runs != null)
            {
                foreach (var run in runs)
                {
                    string text = run.GetText(0);
                    if (string.IsNullOrEmpty(text)) continue;

                    bool runChanged = false;
                    foreach (var kv in map)
                    {
                        if (text.Contains(kv.Key))
                        {
                            text = text.Replace(kv.Key, kv.Value);
                            run.SetText(text, 0); // 原地修改
                            runChanged = true;
                        }
                    }

                    // 如果这个 Run 被修改过，我们标记一下，但这不代表全部完成
                    // 因为可能存在跨 Run 的占位符 (例如 « 在 Run1, Name 在 Run2)
                }
            }

            // 3. 检查是否还有残留占位符 (说明发生了跨 Run 拆分)
            // 如果还有残留，必须使用策略 B：合并 -> 替换 -> 重建 (会丢失部分精细格式，但保证数据正确)
            string currentText = para.ParagraphText;
            foreach (var kv in map)
            {
                if (currentText.Contains(kv.Key))
                {
                    // 进入降级模式
                    ExecuteFallbackReplacement(para, map);
                    break;
                }
            }
        }

        /// <summary>
        /// 降级策略：处理跨 Run 的占位符
        /// 逻辑：提取全文本 -> 替换 -> 删除所有旧 Run -> 写入新 Run
        /// 代价：该段落内被替换部分的独立格式（如局部加粗/变色）会丢失，但段落样式保留
        /// </summary>
        private static void ExecuteFallbackReplacement(XWPFParagraph para, Dictionary<string, string> map)
        {
            string text = para.ParagraphText;

            // 执行所有替换
            foreach (var kv in map)
            {
                if (text.Contains(kv.Key))
                {
                    text = text.Replace(kv.Key, kv.Value);
                }
            }

            while (para.Runs.Count > 0)
            {
                para.RemoveRun(0);
            }

            para.CreateRun().SetText(text);
        }

        #endregion
    }
}