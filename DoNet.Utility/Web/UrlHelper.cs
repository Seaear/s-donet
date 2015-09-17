using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;

namespace DoNet.Utility.Web
{
    /// <summary>
    ///     URL的操作类
    /// </summary>
    public class UrlHelper
    {
        /// <summary>
        ///     添加URL参数
        /// </summary>
        public static string AddParam(string url, string paramName, string value)
        {
            var uri = new Uri(url);
            if (string.IsNullOrEmpty(uri.Query))
            {
                var eval = HttpContext.Current.Server.UrlEncode(value);
                return string.Concat(url, "?" + paramName + "=" + eval);
            }
            else
            {
                var eval = HttpContext.Current.Server.UrlEncode(value);
                return string.Concat(url, "&" + paramName + "=" + eval);
            }
        }

        /// <summary>
        ///     更新URL参数
        /// </summary>
        public static string UpdateParam(string url, string paramName, string value)
        {
            var keyWord = paramName + "=";
            var index = url.IndexOf(keyWord) + keyWord.Length;
            var index1 = url.IndexOf("&", index);
            if (index1 == -1)
            {
                url = url.Remove(index, url.Length - index);
                url = string.Concat(url, value);
                return url;
            }
            url = url.Remove(index, index1 - index);
            url = url.Insert(index, value);
            return url;
        }

        /// <summary>
        ///     分析 url 字符串中的参数信息
        /// </summary>
        /// <param name="url">输入的 URL</param>
        /// <param name="baseUrl">输出 URL 的基础部分</param>
        /// <param name="nvc">输出分析后得到的 (参数名,参数值) 的集合</param>
        public static void ParseUrl(string url, out string baseUrl, out NameValueCollection nvc)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            nvc = new NameValueCollection();
            baseUrl = "";

            if (url == "")
                return;

            var questionMarkIndex = url.IndexOf('?');

            if (questionMarkIndex == -1)
            {
                baseUrl = url;
                return;
            }
            baseUrl = url.Substring(0, questionMarkIndex);
            if (questionMarkIndex == url.Length - 1)
                return;
            var ps = url.Substring(questionMarkIndex + 1);

            // 开始分析参数对    
            var re = new Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", RegexOptions.Compiled);
            var mc = re.Matches(ps);

            foreach (Match m in mc)
            {
                nvc.Add(m.Result("$2").ToLower(), m.Result("$3"));
            }
        }
    }
}