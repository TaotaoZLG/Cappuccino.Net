using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using NPOI.Util;
using NPOI.XWPF.UserModel;
using Xceed.Words.NET;
using static SharpCompress.Compressors.Filters.BranchExecFilter;

namespace Cappuccino.Common.Helpers
{
    /// <summary>
    /// Word文档域值替换帮助类（基于实体DisplayName注解）
    /// 支持 NPOI 2.5.6+、C#7.3 | 新增图片插入功能
    /// </summary>
    public static class NpoiHelper
    {
        // 图片占位符关键字
        private const string IMAGE_PLACEHOLDER = "{图片}";
        private const int MAX_WIDTH = 500000;

        /// <summary>
        /// 批量替换Word文档中的占位符 {占位符名称} + 插入图片
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="wordFilePath">Word文件物理路径</param>
        /// <param name="entity">数据实体</param>
        /// <param name="imagePaths">图片路径集合（可选）</param>
        /// <param name="dateFormat">日期格式化，默认 yyyy-MM-dd</param>
        public static void ReplaceContent<T>(string wordFilePath, T entity, List<string> imagePaths = null, string dateFormat = "yyyy-MM-dd")
        {
            var keyValues = BuildPlaceholderMap(entity, dateFormat);
            // 临时文件避免覆盖原文件（核心：防止原文件被占用导致写入不完整）
            string tempFilePath = Path.Combine(Path.GetDirectoryName(wordFilePath), $"temp_{Guid.NewGuid()}.docx");

            // 1. 读取原文件到内存
            byte[] docBytes = File.ReadAllBytes(wordFilePath);
            using (var ms = new MemoryStream(docBytes))
            {
                XWPFDocument doc = new XWPFDocument(ms);
                ReplaceParagraphs(doc.Paragraphs, keyValues);
                ReplaceTables(doc.Tables, keyValues);
                ReplaceHeadersFooters(doc, keyValues);

                if (imagePaths != null && imagePaths.Count > 0)
                {
                    InsertImagesToWord(doc, imagePaths);
                }

                // 2. 写入临时文件
                using (var outputFs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    doc.Write(outputFs);
                }
            }

            // 3. 替换原文件（确保原文件已释放）
            File.Delete(wordFilePath);
            File.Move(tempFilePath, wordFilePath);
        }

        /// <summary>
        /// NPOI创建Word文档，批量插入图片
        /// </summary>
        public static void CreateWordWithImages(List<string> imagePaths, string savePath)
        {
            // 创建Word文档（.docx格式）
            XWPFDocument document = new XWPFDocument();

            // 添加标题
            //var titleParagraph = document.CreateParagraph();
            //titleParagraph.Alignment = ParagraphAlignment.CENTER;
            //var titleRun = titleParagraph.CreateRun();
            //titleRun.SetText("案件归档图片文档");
            //titleRun.IsBold = true;
            //titleRun.FontSize = 16;

            // 批量插入图片
            foreach (var imgPath in imagePaths)
            {
                // 换行
                document.CreateParagraph();
                var para = document.CreateParagraph();
                para.Alignment = ParagraphAlignment.CENTER;
                var run = para.CreateRun();

                // 插入图片（设置宽度：400px，自适应高度）
                using (FileStream fs = new FileStream(imgPath, FileMode.Open, FileAccess.Read))
                {
                    run.AddPicture(fs, (int)GetPictureType(imgPath), Path.GetFileName(imgPath),
                        Units.ToEMU(400), Units.ToEMU(300));
                }
            }

            // 保存Word文档
            using (FileStream fs = new FileStream(savePath, FileMode.Create))
            {
                document.Write(fs);
            }
        }

        /// <summary>
        /// NPOI提取Word(.docx)第一张图片
        /// 返回：提取结果、临时图片路径、原Word文件名、图片文件名(含扩展名)
        /// </summary>
        /// <param name="wordPath">Word文件路径</param>
        /// <param name="tempImgPath">临时图片保存路径</param>
        /// <returns></returns>
        public static (string result, string tempImgPath, string wordFileName, string imageFileName) ExtractFirstImageFromWord(string wordPath, string tempImagePath)
        {
            try
            {
                if (!File.Exists(wordPath)) return ("Word文件不存在", "", "", "");

                // NPOI仅支持 .docx 格式，不支持旧版 .doc
                string ext = Path.GetExtension(wordPath).ToLower();
                if (ext == ".doc") return ("NPOI不支持.doc格式，请使用.docx文件", "", "", "");

                string wordFileName = Path.GetFileName(wordPath);

                // 1. NPOI加载docx文档
                using (FileStream fs = new FileStream(wordPath, FileMode.Open, FileAccess.Read))
                {
                    XWPFDocument doc = new XWPFDocument(fs);
                    // 获取文档中所有图片
                    var pictures = doc.AllPictures;
                    if (pictures == null || !pictures.Any()) return ("Word中未找到图片", "", "", "");

                    // 2. 仅取第一张图片 + 获取原始文件名
                    var firstPicture = pictures[0];
                    //string imageFileName = firstPicture.FileName; // 原始图片文件名（含扩展名）
                    string tempImageFileName = $"OCR识别图片_{Path.GetFileNameWithoutExtension(wordFileName)}_{Guid.NewGuid():N}.png";  // 临时图片文件名（统一使用PNG格式）
                    byte[] imageBytes = firstPicture.Data; // 图片字节流

                    // 3. 保存临时图片
                    string tempImgPath = Path.Combine(tempImagePath, tempImageFileName);
                    using (var ms = new MemoryStream(imageBytes))
                    using (var img = Image.FromStream(ms))
                    {
                        img.Save(tempImgPath, ImageFormat.Png);
                    }

                    return ("图片提取成功", tempImgPath, wordFileName, tempImageFileName);
                }
            }
            catch (Exception ex)
            {
                return ($"图片提取失败：{ex.Message}", "", "", "");
            }
        }

        #region 图片插入核心功能
        private static void InsertImagesToWord(XWPFDocument doc, List<string> imagePaths)
        {
            const int MAX_WIDTH = 1500000;
            XWPFParagraph targetPara = null;
            foreach (var para in doc.Paragraphs)
            {
                if (para.ParagraphText.Contains(IMAGE_PLACEHOLDER))
                {
                    targetPara = para;
                    break;
                }
            }

            if (targetPara != null)
            {
                while (targetPara.Runs.Count > 0)
                {
                    targetPara.RemoveRun(0);
                }
            }

            foreach (var path in imagePaths)
            {
                if (!File.Exists(path)) continue;

                try
                {
                    // 关键：将图片流读取为字节数组，避免using提前释放
                    byte[] imageBytes = File.ReadAllBytes(path);
                    using (MemoryStream imgStream = new MemoryStream(imageBytes))
                    using (Image image = Image.FromStream(imgStream, true, true)) // 禁止缓存流
                    {
                        int renderHeight = (int)(MAX_WIDTH * image.Height / (double)image.Width);
                        XWPFParagraph para = doc.CreateParagraph();
                        para.Alignment = ParagraphAlignment.CENTER;
                        XWPFRun run = para.CreateRun();

                        // 重置流位置（关键）
                        imgStream.Seek(0, SeekOrigin.Begin);
                        // 插入图片：调整参数类型为long（NPOI 2.5.6 实际接收long，强转int会溢出）
                        run.AddPicture(
                            imgStream,
                            (int)GetPictureType(path),
                            Path.GetFileName(path),
                            MAX_WIDTH, // 直接用long，避免int溢出
                            renderHeight
                        );

                        // 优化段落插入逻辑，减少结构破坏
                        if (targetPara != null)
                        {
                            int targetIndex = doc.Paragraphs.IndexOf(targetPara);
                            // 直接插入到目标位置后，避免循环调整
                            doc.Paragraphs.Insert(targetIndex + 1, para);
                            // 移除最后自动创建的空段落（NPOI CreateParagraph的副作用）
                            if (doc.Paragraphs.Count > targetIndex + 2)
                            {
                                doc.Paragraphs.RemoveAt(doc.Paragraphs.Count - 1);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 打印异常便于排查（临时调试用）
                    Console.WriteLine($"插入图片失败：{path}，错误：{ex.Message}");
                    continue;
                }
            }
        }

        /// <summary>
        /// 获取NPOI 2.5.6兼容的图片类型
        /// </summary>
        private static PictureType GetPictureType(string fileName)
        {
            string ext = Path.GetExtension(fileName).TrimStart('.').ToLower();
            PictureType picType = PictureType.PNG;

            switch (ext)
            {
                case "jpg":
                case "jpeg":
                    picType = PictureType.JPEG;
                    break;
                case "png":
                    picType = PictureType.PNG;
                    break;
                case "bmp":
                    picType = PictureType.BMP;
                    break;
                case "gif":
                    picType = PictureType.GIF;
                    break;
                default:
                    picType = PictureType.PNG;
                    break;
            }
            return picType;
        }
        #endregion

        #region 原有私有核心方法（完全保留，无修改）
        /// <summary>
        /// 构建实体属性 => {DisplayName} 映射字典
        /// </summary>
        private static Dictionary<string, string> BuildPlaceholderMap<T>(T entity, string dateFormat)
        {
            var map = new Dictionary<string, string>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var displayAttr = prop.GetCustomAttribute<DisplayNameAttribute>();
                if (displayAttr == null || string.IsNullOrWhiteSpace(displayAttr.DisplayName))
                    continue;

                object value = prop.GetValue(entity);
                string strValue = string.Empty;

                if (value != null)
                {
                    if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                    {
                        strValue = ((DateTime)value).ToString(dateFormat);
                    }
                    else
                    {
                        strValue = value.ToString();
                    }
                }

                string key = $"{{{displayAttr.DisplayName}}}";
                if (map.ContainsKey(key))
                    map[key] = strValue;
                else
                    map.Add(key, strValue);
            }
            return map;
        }

        private static void ReplaceParagraphs(IList<XWPFParagraph> paragraphs, Dictionary<string, string> map)
        {
            if (paragraphs == null) return;
            foreach (var para in paragraphs)
            {
                ReplaceSingleParagraph(para, map);
            }
        }

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

        private static void ReplaceHeadersFooters(XWPFDocument doc, Dictionary<string, string> map)
        {
            if (doc.HeaderList != null)
            {
                foreach (var header in doc.HeaderList)
                {
                    ReplaceParagraphs(header.Paragraphs, map);
                }
            }
            if (doc.FooterList != null)
            {
                foreach (var footer in doc.FooterList)
                {
                    ReplaceParagraphs(footer.Paragraphs, map);
                }
            }
        }

        private static void ReplaceSingleParagraph(XWPFParagraph para, Dictionary<string, string> map)
        {
            if (para == null) return;
            string fullText = para.ParagraphText;
            if (string.IsNullOrWhiteSpace(fullText)) return;

            bool hasPlaceholder = map.Keys.Any(k => fullText.Contains(k));
            if (!hasPlaceholder) return;

            var runs = para.Runs;
            if (runs != null)
            {
                foreach (var run in runs)
                {
                    string text = run.GetText(0);
                    if (string.IsNullOrEmpty(text)) continue;

                    foreach (var kv in map)
                    {
                        if (text.Contains(kv.Key))
                        {
                            text = text.Replace(kv.Key, kv.Value);
                            run.SetText(text, 0);
                        }
                    }
                }
            }

            string currentText = para.ParagraphText;
            foreach (var kv in map)
            {
                if (currentText.Contains(kv.Key))
                {
                    ExecuteFallbackReplacement(para, map);
                    break;
                }
            }
        }

        private static void ExecuteFallbackReplacement(XWPFParagraph para, Dictionary<string, string> map)
        {
            string text = para.ParagraphText;
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
    public class DocxHelper
    {
        /// <summary>
        /// 创建Word并插入所有图片（单集合遍历）
        /// </summary>
        public static void CreateWordWithImages(string wordPath, string userName, string cardNo, List<string> allImages)
        {
            using (var document = DocX.Create(wordPath))
            {
                // 遍历【唯一图片集合】插入所有图片
                foreach (var imgPath in allImages)
                {
                    try
                    {
                        // 插入图片
                        var p = document.InsertParagraph();
                        p.AppendPicture(document.AddImage(imgPath).CreatePicture());
                        // 插入图片文件名备注
                        document.InsertParagraph($"文件名：{Path.GetFileName(imgPath)}").FontSize(9).Italic();
                        document.InsertParagraph();
                    }
                    catch
                    {
                        // 图片损坏/无法读取则跳过
                        continue;
                    }
                }

                // 保存Word到主文件夹
                document.Save();
            }
        }

        /// <summary>
        /// 从Word中提取第一张图片
        /// </summary>
        public static string ExtractFirstImageFromWordAndOcr(string wordPath)
        {
            try
            {
                if (!File.Exists(wordPath)) return "Word文件不存在";

                // 读取Word，提取第一张图片
                using (var doc = DocX.Load(wordPath))
                {
                    var images = doc.Images.ToList();
                    if (!images.Any()) return "Word中未找到图片";

                    // 仅取第一张图片
                    var firstImage = images.First();
                    string tempImgPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");

                    // 通过GetStream方法获取图片流
                    using (var imgStream = firstImage.GetStream(FileMode.Open, FileAccess.Read))
                    using (var img = Image.FromStream(imgStream))
                    {
                        img.Save(tempImgPath, ImageFormat.Png);
                    }

                    return tempImgPath;
                }
            }
            catch (Exception ex)
            {
                return $"OCR识别失败：{ex.Message}";
            }
        }
    }
}