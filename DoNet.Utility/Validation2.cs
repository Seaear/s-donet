﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;

namespace DoNet.Utility
{
    public class Validation2
    {
        private static readonly Regex RegPhone = new Regex("^[0-9]+[-]?[0-9]+[-]?[0-9]$");
        private static readonly Regex RegNumber = new Regex("^[0-9]+$");
        private static readonly Regex RegNumberSign = new Regex("^[+-]?[0-9]+$");
        private static readonly Regex RegDecimal = new Regex("^[0-9]+[.]?[0-9]+$");
        private static readonly Regex RegDecimalSign = new Regex("^[+-]?[0-9]+[.]?[0-9]+$"); //等价于^[+-]?\d+[.]?\d+$

        private static readonly Regex RegEmail = new Regex("^[\\w-]+@[\\w-]+\\.(com|net|org|edu|mil|tv|biz|info)$");
        //w 英文字母或数字的字符串，和 [a-zA-Z0-9] 语法一样 

        private static readonly Regex RegCHZN = new Regex("[\u4e00-\u9fa5]");

        #region 中文检测

        /// <summary>
        ///     检测是否有中文字符
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static bool IsHasCHZN(string inputData)
        {
            var m = RegCHZN.Match(inputData);
            return m.Success;
        }

        #endregion

        #region 邮件地址

        /// <summary>
        ///     是否是浮点数 可带正负号
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns></returns>
        public static bool IsEmail(string inputData)
        {
            var m = RegEmail.Match(inputData);
            return m.Success;
        }

        #endregion

        #region 日期格式判断

        /// <summary>
        ///     日期格式字符串判断
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDateTime(string str)
        {
            try
            {
                if (!string.IsNullOrEmpty(str))
                {
                    DateTime.Parse(str);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 检查输入的参数是不是某些定义好的特殊字符：这个方法目前用于密码输入的安全检查

        /// <summary>
        ///     检查输入的参数是不是某些定义好的特殊字符：这个方法目前用于密码输入的安全检查
        /// </summary>
        public static bool IsContainSpecChar(string strInput)
        {
            string[] list = {"123456", "654321"};
            var result = new bool();
            for (var i = 0; i < list.Length; i++)
            {
                if (strInput == list[i])
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        #endregion

        #region 数字字符串检查		

        public static bool IsPhone(string inputData)
        {
            var m = RegPhone.Match(inputData);
            return m.Success;
        }

        /// <summary>
        ///     检查Request查询字符串的键值，是否是数字，最大长度限制
        /// </summary>
        /// <param name="req">Request</param>
        /// <param name="inputKey">Request的键值</param>
        /// <param name="maxLen">最大长度</param>
        /// <returns>返回Request查询字符串</returns>
        public static string FetchInputDigit(HttpRequest req, string inputKey, int maxLen)
        {
            var retVal = string.Empty;
            if (!string.IsNullOrEmpty(inputKey))
            {
                retVal = req.QueryString[inputKey];
                if (null == retVal)
                    retVal = req.Form[inputKey];
                if (null != retVal)
                {
                    retVal = SqlText(retVal, maxLen);
                    if (!IsNumber(retVal))
                        retVal = string.Empty;
                }
            }
            if (retVal == null)
                retVal = string.Empty;
            return retVal;
        }

        /// <summary>
        ///     是否数字字符串
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns></returns>
        public static bool IsNumber(string inputData)
        {
            var m = RegNumber.Match(inputData);
            return m.Success;
        }

        /// <summary>
        ///     是否数字字符串 可带正负号
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns></returns>
        public static bool IsNumberSign(string inputData)
        {
            var m = RegNumberSign.Match(inputData);
            return m.Success;
        }

        /// <summary>
        ///     是否是浮点数
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns></returns>
        public static bool IsDecimal(string inputData)
        {
            var m = RegDecimal.Match(inputData);
            return m.Success;
        }

        /// <summary>
        ///     是否是浮点数 可带正负号
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns></returns>
        public static bool IsDecimalSign(string inputData)
        {
            var m = RegDecimalSign.Match(inputData);
            return m.Success;
        }

        #endregion

        #region 其他

        /// <summary>
        ///     检查字符串最大长度，返回指定长度的串
        /// </summary>
        /// <param name="sqlInput">输入字符串</param>
        /// <param name="maxLength">最大长度</param>
        /// <returns></returns>
        public static string SqlText(string sqlInput, int maxLength)
        {
            if (sqlInput != null && sqlInput != string.Empty)
            {
                sqlInput = sqlInput.Trim();
                if (sqlInput.Length > maxLength) //按最大长度截取字符串
                    sqlInput = sqlInput.Substring(0, maxLength);
            }
            return sqlInput;
        }

        /// <summary>
        ///     字符串编码
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static string HtmlEncode(string inputData)
        {
            return HttpUtility.HtmlEncode(inputData);
        }

        /// <summary>
        ///     设置Label显示Encode的字符串
        /// </summary>
        /// <param name="lbl"></param>
        /// <param name="txtInput"></param>
        public static void SetLabel(Label lbl, string txtInput)
        {
            lbl.Text = HtmlEncode(txtInput);
        }

        public static void SetLabel(Label lbl, object inputObj)
        {
            SetLabel(lbl, inputObj.ToString());
        }

        //字符串清理
        public static string InputText(string inputString, int maxLength)
        {
            var retVal = new StringBuilder();

            // 检查是否为空
            if (!string.IsNullOrEmpty(inputString))
            {
                inputString = inputString.Trim();

                //检查长度
                if (inputString.Length > maxLength)
                    inputString = inputString.Substring(0, maxLength);

                //替换危险字符
                for (var i = 0; i < inputString.Length; i++)
                {
                    switch (inputString[i])
                    {
                        case '"':
                            retVal.Append("&quot;");
                            break;
                        case '<':
                            retVal.Append("&lt;");
                            break;
                        case '>':
                            retVal.Append("&gt;");
                            break;
                        default:
                            retVal.Append(inputString[i]);
                            break;
                    }
                }
                retVal.Replace("'", " "); // 替换单引号
            }
            return retVal.ToString();
        }

        /// <summary>
        ///     转换成 HTML code
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>string</returns>
        public static string Encode(string str)
        {
            str = str.Replace("&", "&amp;");
            str = str.Replace("'", "''");
            str = str.Replace("\"", "&quot;");
            str = str.Replace(" ", "&nbsp;");
            str = str.Replace("<", "&lt;");
            str = str.Replace(">", "&gt;");
            str = str.Replace("\n", "<br>");
            return str;
        }

        /// <summary>
        ///     解析html成 普通文本
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>string</returns>
        public static string Decode(string str)
        {
            str = str.Replace("<br>", "\n");
            str = str.Replace("&gt;", ">");
            str = str.Replace("&lt;", "<");
            str = str.Replace("&nbsp;", " ");
            str = str.Replace("&quot;", "\"");
            return str;
        }

        public static string SqlTextClear(string sqlText)
        {
            if (sqlText == null)
            {
                return null;
            }
            if (sqlText == "")
            {
                return "";
            }
            sqlText = sqlText.Replace(",", ""); //去除,
            sqlText = sqlText.Replace("<", ""); //去除<
            sqlText = sqlText.Replace(">", ""); //去除>
            sqlText = sqlText.Replace("--", ""); //去除--
            sqlText = sqlText.Replace("'", ""); //去除'
            sqlText = sqlText.Replace("\"", ""); //去除"
            sqlText = sqlText.Replace("=", ""); //去除=
            sqlText = sqlText.Replace("%", ""); //去除%
            sqlText = sqlText.Replace(" ", ""); //去除空格
            return sqlText;
        }

        #endregion

        #region 是否由特定字符组成

        public static bool IsContainSameChar(string strInput)
        {
            var charInput = string.Empty;
            if (!string.IsNullOrEmpty(strInput))
            {
                charInput = strInput.Substring(0, 1);
            }
            return strInput != null && IsContainSameChar(strInput, charInput, strInput.Length);
        }

        public static bool IsContainSameChar(string strInput, string charInput, int lenInput)
        {
            if (string.IsNullOrEmpty(charInput))
            {
                return false;
            }
            var regNumber = new Regex(string.Format("^([{0}])+$", charInput));
            //Regex RegNumber = new Regex(string.Format("^([{0}]{{1}})+$", charInput,lenInput));
            var m = regNumber.Match(strInput);
            return m.Success;
        }

        #endregion
    }
}