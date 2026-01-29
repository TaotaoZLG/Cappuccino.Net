using System;
using System.Security.Cryptography;
using System.Text;

namespace Cappuccino.Common.Util
{
    /// <summary>
    /// DES加密解密
    /// </summary>
    public class DESUtils
    {
        private static string DESKey = "Cappuccino@DESKey";

        // 新增：用于生成8字节的DES密钥和IV（MD5哈希前8字节）
        private static byte[] GetDesKeyBytes(string sKey)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sKey));
                byte[] keyBytes = new byte[8];
                Array.Copy(hash, keyBytes, 8);
                return keyBytes;
            }
        }

        #region ========加密========
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string Encrypt(string Text)
        {
            return Encrypt(Text, DESKey);
        }
        /// <summary> 
        /// 加密数据 
        /// </summary> 
        /// <param name="Text"></param> 
        /// <param name="sKey"></param> 
        /// <returns></returns> 
        public static string Encrypt(string Text, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.Default.GetBytes(Text);
            byte[] keyBytes = GetDesKeyBytes(sKey);
            des.Key = keyBytes;
            des.IV = keyBytes;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        #endregion

        #region ========解密========
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string Decrypt(string Text)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                return Decrypt(Text, DESKey);
            }
            else
            {
                return "";
            }
        }
        /// <summary> 
        /// 解密数据 
        /// </summary> 
        /// <param name="Text"></param> 
        /// <param name="sKey"></param> 
        /// <returns></returns> 
        public static string Decrypt(string Text, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            int len = Text.Length / 2;
            byte[] inputByteArray = new byte[len];
            for (int x = 0; x < len; x++)
            {
                int i = Convert.ToInt32(Text.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }
            byte[] keyBytes = GetDesKeyBytes(sKey);
            des.Key = keyBytes;
            des.IV = keyBytes;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }

        #endregion
    }
}
