using System;
using System.Collections.Generic;

namespace DoNet.Utility
{
    public sealed class CharAlgorithm
    {
        /// <summary>
        ///     从字符串里，随机得到，规定个数的字符串
        /// </summary>
        /// <returns></returns>
        public string Random(List<char> listChar, int count)
        {
            //charArray = "1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,i,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z"; 
            var randomCode = "";
            var temp = -1;
            var rand = new Random();
            for (var i = 0; i < count; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(temp*i*((int) DateTime.Now.Ticks));
                }

                var t = rand.Next(listChar.Count - 1);

                while (temp == t)
                {
                    t = rand.Next(listChar.Count - 1);
                }

                temp = t;
                randomCode += listChar[t];
            }
            return randomCode;
        }

        /// <summary>
        ///     从字符串里，得到所有，规定个数的字符串
        /// </summary>
        /// <returns></returns>
        public List<string> Permute(List<char> listChar, int count, string temp = null,
            List<string> result = null)
        {
            if (temp == null) temp = string.Empty;
            if (result == null) result = new List<string>();
            if (listChar == null || listChar.Count <= 0) return result;
            foreach (var c in listChar)
            {
                var value = temp + c;
                if (count == 1)
                {
                    result.Add(value);
                }
                else
                {
                    Permute(listChar, --count, value, result);
                }
            }
            return result;
        }
    }
}