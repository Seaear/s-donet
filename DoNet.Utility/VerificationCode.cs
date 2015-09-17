using System;
using System.Threading;

namespace DoNet.Utility
{
    /// <summary>
    ///     ��֤����
    /// </summary>
    public class VerificationCode
    {
        #region �����������

        /// <summary>
        ///     �����������
        /// </summary>
        public static string Number(int length)
        {
            return Number(length, false);
        }

        /// <summary>
        ///     �����������
        /// </summary>
        /// <param name="length">���ɳ���</param>
        /// <param name="sleep">�Ƿ�Ҫ������ǰ����ǰ�߳���ֹ�Ա����ظ�</param>
        public static string Number(int length, bool sleep)
        {
            if (sleep) Thread.Sleep(3);
            var result = "";
            var random = new Random();
            for (var i = 0; i < length; i++)
            {
                result += random.Next(10).ToString();
            }
            return result;
        }

        #endregion

        #region ���������ĸ������

        /// <summary>
        ///     ���������ĸ������
        /// </summary>
        public static string Str(int length)
        {
            return Str(length, false);
        }

        /// <summary>
        ///     ���������ĸ������
        /// </summary>
        public static string Str(int length, bool sleep)
        {
            if (sleep) Thread.Sleep(3);
            char[] pattern =
            {
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H',
                'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
            };
            var result = "";
            var n = pattern.Length;
            var random = new Random(~unchecked((int)DateTime.Now.Ticks));
            for (var i = 0; i < length; i++)
            {
                var rnd = random.Next(0, n);
                result += pattern[rnd];
            }
            return result;
        }

        #endregion

        #region �����������ĸ�����

        /// <summary>
        ///     �����������ĸ�����
        /// </summary>
        public static string Str_char(int length)
        {
            return Str_char(length, false);
        }

        /// <summary>
        ///     �����������ĸ�����
        /// </summary>
        /// <param name="length">���ɳ���</param>
        /// <param name="sleep">�Ƿ�Ҫ������ǰ����ǰ�߳���ֹ�Ա����ظ�</param>
        public static string Str_char(int length, bool sleep)
        {
            if (sleep) Thread.Sleep(3);
            char[] pattern =
            {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R',
                'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
            };
            var result = "";
            var n = pattern.Length;
            var random = new Random(~unchecked((int)DateTime.Now.Ticks));
            for (var i = 0; i < length; i++)
            {
                var rnd = random.Next(0, n);
                result += pattern[rnd];
            }
            return result;
        }

        #endregion
    }
}