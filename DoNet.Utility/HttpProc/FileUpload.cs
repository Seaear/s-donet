using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DoNet.Utility.HttpProc
{
    // 使用put进行上传，IIS服务器需设置：
    // 1、开启WebDav功能，添加创作规则。
    // 2、开启目录浏览功能。
    // 3、在身份验证中开启Windows身份验证，关闭匿名验证。
    // 4、在webconfig中加入<httpRuntime maxRequestLength="10240" />控制附件大小。

    public class FileUpload
    {
        public FileUpload(string userName, string password)
        {
            FileServerName = userName;
            FileServerPassword = password;
        }

        /// <summary>
        ///     文件服务器用户名
        /// </summary>
        public string FileServerName { get; set; }

        /// <summary>
        ///     文件服务器密码
        /// </summary>
        public string FileServerPassword { get; set; }

        /// <summary>
        ///     获取文件服务器上指定路径的文件列表
        /// </summary>
        public string[] GetFiles(string filePathUrl)
        {
            if (string.IsNullOrEmpty(filePathUrl))
                return null;
            using (var webClient = GetWebClient())
            {
                var pageData = webClient.DownloadData(filePathUrl);
                var response = Encoding.UTF8.GetString(pageData).ToLower();
                const string pattern = "href=\"" + "([^\\.><]{1,}\\.[^\\.><]{1,})\"";
                var matchCollection = Regex.Matches(response, pattern);
                var list = new List<string>(1024);
                if (matchCollection.Count > 0)
                {
                    for (var index = 0; index < matchCollection.Count; ++index)
                    {
                        var filePath = matchCollection[index].Groups[1].Value;
                        var len = filePath.LastIndexOf("/", StringComparison.Ordinal);
                        if (len >= 0) filePath = filePath.Substring(len + 1);
                        list.Add(filePath);
                    }
                }
                return list.Count == 0 ? null : list.ToArray();
            }
        }

        /// <summary>
        ///     读取文件服务器上指定文件
        /// </summary>
        public byte[] ReadFileFromServer(string fileNameUrl)
        {
            if (string.IsNullOrEmpty(fileNameUrl))
                return null;
            try
            {
                using (var webClient = GetWebClient())
                {
                    return webClient.DownloadData(fileNameUrl);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        ///     上传文件到文件服务器上指定路径
        /// </summary>
        public void UploadFileToServer(string fileNameUrl, byte[] fileData)
        {
            CheckCreatFolder(GetFatherFolder(fileNameUrl));
            using (var webClient = GetWebClient())
            {
                webClient.UploadData(fileNameUrl, "PUT", fileData);
            }
        }

        private WebClient GetWebClient()
        {
            var webClient = new WebClient();
            webClient.Credentials = new NetworkCredential(FileServerName, FileServerPassword);
            return webClient;
        }

        private void CheckCreatFolder(string url)
        {
            var stack = new Stack<string>(16);
            if (url.EndsWith("/"))
                url = url.Remove(url.Length - 1, 1);
            int length;
            for (; !CheckFolderExist(url); url = url.Substring(0, length))
            {
                stack.Push(url);
                length = url.LastIndexOf("/");
            }
            while (stack.Count > 0)
                CreatFolder(stack.Pop());
        }

        private bool CheckFolderExist(string url)
        {
            try
            {
                using (var webClient = GetWebClient())
                {
                    webClient.DownloadData(url);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetFatherFolder(string url)
        {
            var num = url.LastIndexOf(".");
            var length = url.LastIndexOf("/");
            if (num < 0 || num < length)
                return url;
            return url.Substring(0, length);
        }

        private void CreatFolder(string url)
        {
            try
            {
                using (var webClient = GetWebClient())
                {
                    webClient.UploadData(url, "MKCOL", new byte[0]);
                }
            }
            catch
            {
                // ignored
            }
        }
    }

}