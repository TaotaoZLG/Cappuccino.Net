using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cappuccino.Common.Helper
{
    public class OtherHelper
    {
        #region 根据身份证计算年龄和性别
        /// <summary>
        /// 身份证解析：获取性别、出生日期（兼容18位/15位）
        /// </summary>
        public static (string Gender, string BirthDate) ParseIdCard(string idCard)
        {
            string gender = string.Empty;
            string birthDate = string.Empty;

            if (string.IsNullOrWhiteSpace(idCard)) return (gender, birthDate);

            try
            {
                // 18位身份证
                if (idCard.Length == 18)
                {
                    // 出生日期：第7-14位 yyyyMMdd
                    birthDate = DateTime.ParseExact(idCard.Substring(6, 8), "yyyyMMdd", null).ToString("yyyy-MM-dd");
                    // 性别：第17位 奇数男 偶数女
                    int genderCode = int.Parse(idCard.Substring(14, 1));
                    gender = genderCode % 2 == 1 ? "男" : "女";
                }
                // 15位身份证（老版）
                else if (idCard.Length == 15)
                {
                    // 出生日期：第7-12位 yyMMdd
                    birthDate = DateTime.ParseExact($"19{idCard.Substring(6, 6)}", "yyyyMMdd", null).ToString("yyyy-MM-dd");
                    // 性别：第15位 奇数男 偶数女
                    int genderCode = int.Parse(idCard.Substring(14, 1));
                    gender = genderCode % 2 == 1 ? "男" : "女";
                }
            }
            catch
            {
                // 身份证格式错误，返回空
                gender = "格式错误";
                birthDate = "格式错误";
            }

            return (gender, birthDate);
        }
        #endregion
    }
}
