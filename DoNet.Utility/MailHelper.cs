using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace DoNet.Utility
{
    class MailHelper
    {
        /// <summary>
        /// 发送邮件，如果发送成功则返回空，如果发送失败则返回错误信息。
        /// </summary>
        public static string SendMail(string host, int port, string from, string password, string to, string subject, string body)
        {
            host = "smtp.126.com";
            port = 25;
            from = "st_baby@126.com";
            password = "163_sb@0090";
            to = "stone0090@qq.com";
            subject = "测试邮件";
            body = "测试邮件正文";

            string result = string.Empty;

            try
            {
                MailMessage myMessage = new MailMessage();
                myMessage.From = new MailAddress(from);
                myMessage.BodyEncoding = Encoding.UTF8;
                myMessage.Subject = subject;
                myMessage.Body = body;
                myMessage.To.Add(to);

                SmtpClient myClient = new SmtpClient();
                myClient.Host = host;
                myClient.Port = port;
                myClient.Credentials = new NetworkCredential(from, password);
                myClient.Send(myMessage);
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }

            return result;
        }
    }
}
