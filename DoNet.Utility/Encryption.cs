using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DoNet.Utility
{
    /// <summary>
    ///     使用DES加密解密字符串
    /// </summary>
    public static class Encryption
    {
        private static readonly byte[] StrDesKey = {0x2e, 0x3f, 0x83, 0xc9, 0x22, 0x8e, 0x92, 0x88};
        private static readonly byte[] StrDesIv = {0xc3, 0x22, 0x06, 0x9a, 0x3b, 0x52, 0x92, 0xf5};

        /// <summary>
        ///     判断字符串是否经过加密
        /// </summary>
        public static bool IsEncrypted(string val)
        {
            if (!Regex.IsMatch(val, "^[0-9,a-f,A-F]{2,}$"))
                return false;
            if ((val.Length%2) != 0)
                return false;
            return true;
        }

        /// <summary>
        ///     加密
        /// </summary>
        public static string Encrypt(string val)
        {
            if (string.IsNullOrEmpty(val))
                return "";

            var srcData = Encoding.UTF8.GetBytes(val);
            var destData = DesEncrypt(srcData, StrDesKey, StrDesIv);
            var retBuffer = new StringBuilder();
            for (var i = 0; i < destData.Length; i++)
            {
                retBuffer.Append(destData[i].ToString("x2"));
            }

            return retBuffer.ToString();
        }

        /// <summary>
        ///     解密
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string Decrypt(string val)
        {
            if (string.IsNullOrEmpty(val))
                return "";
            if (!Regex.IsMatch(val, "^[0-9,a-f,A-F]{2,}$"))
                return "";

            using (var ms = new MemoryStream(val.Length))
            {
                var tmpIndex = 0;
                while (tmpIndex < (val.Length - 1))
                {
                    ms.WriteByte(byte.Parse(val.Substring(tmpIndex, 2), NumberStyles.HexNumber));
                    tmpIndex += 2;
                }
                var data = ms.ToArray();
                var destData = DesDecrypt(data, StrDesKey, StrDesIv);
                return Encoding.UTF8.GetString(destData);
            }
        }

        /// <summary>
        ///     使用DES加密
        /// </summary>
        /// <param name="srcData">源数据</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">向量</param>
        /// <returns>密文</returns>
        private static byte[] DesEncrypt(byte[] srcData, byte[] key, byte[] iv)
        {
            using (var desEncrypt = new DESCryptoServiceProvider())
            {
                using (var msEncrypt = new MemoryStream())
                {
                    using (
                        var csEncrypt = new CryptoStream(msEncrypt, desEncrypt.CreateEncryptor(key, iv),
                            CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(srcData, 0, srcData.Length);
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        /// <summary>
        ///     DES解密
        /// </summary>
        /// <param name="cipherData">密文</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">向量</param>
        /// <returns>明文</returns>
        private static byte[] DesDecrypt(byte[] cipherData, byte[] key, byte[] iv)
        {
            using (var desDecrypt = new DESCryptoServiceProvider())
            {
                using (var msCipherData = new MemoryStream(cipherData))
                {
                    using (var msRawData = new MemoryStream())
                    {
                        var buffer = new byte[512];
                        using (
                            var csDecrypt = new CryptoStream(msCipherData, desDecrypt.CreateDecryptor(key, iv),
                                CryptoStreamMode.Read))
                        {
                            int readCount;
                            while ((readCount = csDecrypt.Read(buffer, 0, 512)) > 0)
                            {
                                msRawData.Write(buffer, 0, readCount);
                            }
                            return msRawData.ToArray();
                        }
                    }
                }
            }
        }
    }
}