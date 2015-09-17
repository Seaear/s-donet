using System.Collections.Generic;
using System.Text;

namespace DoNet.Utility
{
    public class StringHelper
    {
        public static List<string> GetStrArray(string str, char speater, bool toLower)
        {
            var list = new List<string>();
            var ss = str.Split(speater);
            foreach (var s in ss)
            {
                if (!string.IsNullOrEmpty(s) && s != speater.ToString())
                {
                    var strVal = s;
                    if (toLower)
                    {
                        strVal = s.ToLower();
                    }
                    list.Add(strVal);
                }
            }
            return list;
        }

        public static string[] GetStrArray(string str)
        {
            return str.Split(new char[',']);
        }

        public static string GetArrayStr(List<string> list, string speater)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < list.Count; i++)
            {
                if (i == list.Count - 1)
                {
                    sb.Append(list[i]);
                }
                else
                {
                    sb.Append(list[i]);
                    sb.Append(speater);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        ///     删除最后结尾的一个逗号
        /// </summary>
        public static string DelLastComma(string str)
        {
            return str.Substring(0, str.LastIndexOf(","));
        }

        /// <summary>
        ///     删除最后结尾的指定字符后的字符
        /// </summary>
        public static string DelLastChar(string str, string strchar)
        {
            return str.Substring(0, str.LastIndexOf(strchar));
        }

        /// <summary>
        ///     转全角的函数(SBC case)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToSBC(string input)
        {
            //半角转全角：
            var c = input.ToCharArray();
            for (var i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char) 12288;
                    continue;
                }
                if (c[i] < 127)
                    c[i] = (char) (c[i] + 65248);
            }
            return new string(c);
        }

        /// <summary>
        ///     转半角的函数(SBC case)
        /// </summary>
        /// <param name="input">输入</param>
        /// <returns></returns>
        public static string ToDBC(string input)
        {
            var c = input.ToCharArray();
            for (var i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char) 32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char) (c[i] - 65248);
            }
            return new string(c);
        }
    }
}