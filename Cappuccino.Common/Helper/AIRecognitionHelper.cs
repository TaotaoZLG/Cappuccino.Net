using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Cappuccino.Common.Extensions;
using Cappuccino.Common.Util;
using Newtonsoft.Json;

namespace Cappuccino.Common.Helper
{
    /// <summary>
    /// AI识别工具类（对接本地OwenVL8B）
    /// </summary>
    public static class AIRecognitionHelper
    {
        /// <summary>
        /// 身份证识别结果实体
        /// </summary>
        public class IdCardInfo
        {
            public string Name { get; set; } // 姓名
            public string IdNumber { get; set; } // 身份证号
            public string Nation { get; set; } // 民族
            public string Address { get; set; } // 地址
            public string BirthDate { get; set; } // 出生日期
            public string Gender { get; set; } // 性别
        }

        /// <summary>
        /// 图片识别结果实体
        /// </summary>
        public class AIRecognitionResult
        {
            public string ImagePath { get; set; } // 图片路径
            public string ImageType { get; set; } // 图片类型（身份证正/反、协议等）
            public IdCardInfo IdCardInfo { get; set; } // 身份证信息（仅图片类型为身份证时有效）
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        /// <summary>
        /// 调用OwenVL8B识别图片
        /// </summary>
        /// <param name="imagePaths">有效图片路径列表</param>
        /// <returns>识别结果列表</returns>
        public static List<AIRecognitionResult> RecognizeByOwenVL8B(List<string> imagePaths)
        {
            var results = new List<AIRecognitionResult>();
            foreach (var imagePath in imagePaths)
            {
                var result = new AIRecognitionResult { ImagePath = imagePath };
                try
                {
                    var endpoint = ConfigUtils.AppSetting.GetValue("AIService");
                    var timeout = ConfigUtils.AppSetting.GetValue("AITimeout").ParseToInt();
                    var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeout) };

                    // ========== 核心：对接本地OwenVL8B ==========
                    // 1. 若OwenVL8B提供DLL：引用DLL后调用接口
                    // var owenVL8B = new OwenVL8B.SDK();
                    // var recognizeResult = owenVL8B.RecognizeImage(imagePath);

                    // 2. 若OwenVL8B提供HTTP接口：通过HttpClient调用
                    // var client = new HttpClient();
                    // var postData = new { imagePath = imagePath };
                    // var response = client.PostAsync("http://本地OwenVL8B地址/recognize", new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json")).Result;
                    // var recognizeResult = JsonConvert.DeserializeObject<OwenVL8BResult>(response.Content.ReadAsStringAsync().Result);

                    // 模拟识别结果（需替换为真实调用逻辑）
                    if (imagePath.Contains("idcard_front"))
                    {
                        result.ImageType = "身份证正面";
                        result.IdCardInfo = new IdCardInfo
                        {
                            Name = "张三",
                            IdNumber = "110101199001011234",
                            Nation = "汉",
                            Address = "北京市朝阳区XX路XX号",
                            BirthDate = "1990-01-01",
                            Gender = "男"
                        };
                    }
                    else if (imagePath.Contains("idcard_back"))
                    {
                        result.ImageType = "身份证反面";
                    }
                    else if (imagePath.Contains("agreement"))
                    {
                        result.ImageType = "协议";
                    }
                    else
                    {
                        result.ImageType = "未知";
                    }
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }
                results.Add(result);
            }
            return results;
        }

        /// <summary>
        /// 调用本地OwenVL8B识别图片
        /// </summary>
        public static async Task<AIRecognitionResult> RecognizeImageAsync(string imagePath, string batchId, IProgress<ProcessProgress> progress)
        {
            var endpoint = ConfigUtils.AppSetting.GetValue("AIService");
            var timeout = ConfigUtils.AppSetting.GetValue("AITimeout").ParseToInt();
            var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeout) };

            try
            {
                // 构造请求（根据OwenVL8B实际接口调整）
                var requestModel = new { ImagePath = imagePath };
                var json = JsonConvert.SerializeObject(requestModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                progress.Report(new ProcessProgress
                {
                    BatchId = batchId,
                    Type = "Recognize",
                    Progress = 0,
                    Message = $"开始识别图片：{imagePath}"
                });

                //var response = await client.PostAsync(endpoint, content);
                //response.EnsureSuccessStatusCode();
                //var responseStr = await response.Content.ReadAsStringAsync();
                //var result = JsonConvert.DeserializeObject<AIRecognitionResult>(responseStr);

                var result = new AIRecognitionResult { 
                    Success = true,
                    Message = "识别成功（模拟结果）",
                    ImageType = "身份证正面",
                    IdCardInfo = new IdCardInfo
                    {
                        Name = "张三",
                        IdNumber = "110101199001011234",
                        Nation = "汉",
                        Address = "北京市朝阳区XX路XX号",
                        BirthDate = "1990-01-01",
                        Gender = "男"
                    }
                };

                progress.Report(new ProcessProgress
                {
                    BatchId = batchId,
                    Type = "Recognize",
                    Progress = 100,
                    Message = $"识别完成：{imagePath}，类型：{result.ImageType}"
                });
                return result;
            }
            catch (Exception ex)
            {
                progress.Report(new ProcessProgress
                {
                    BatchId = batchId,
                    Type = "Recognize",
                    Progress = 0,
                    Message = $"识别失败：{imagePath}，原因：{ex.Message}"
                });
                return new AIRecognitionResult { Success = false, Message = ex.Message };
            }
        }
    }
}