using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DoNet.Utility.HttpProc
{
    public class MyWebClient
    {
        private static CookieContainer _cc;

        static MyWebClient()
        {
            LoadCookiesFromDisk();
        }

        /// <summary>
        ///     创建WebClient的实例
        /// </summary>
        public MyWebClient()
        {
            RequestHeaders = new WebHeaderCollection();
            ResponseHeaders = new WebHeaderCollection();
        }

        /// <summary>
        ///     设置发送和接收的数据缓冲大小
        /// </summary>
        public int BufferSize { get; set; } = 15240;

        /// <summary>
        ///     获取响应头集合
        /// </summary>
        public WebHeaderCollection ResponseHeaders { get; private set; }

        /// <summary>
        ///     获取请求头集合
        /// </summary>
        public WebHeaderCollection RequestHeaders { get; }

        /// <summary>
        ///     获取或设置代理
        /// </summary>
        public WebProxy Proxy { get; set; }

        /// <summary>
        ///     获取或设置请求与响应的文本编码方式
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.Default;

        /// <summary>
        ///     获取或设置响应的html代码
        /// </summary>
        public string RespHtml { get; set; } = "";

        /// <summary>
        ///     获取或设置与请求关联的Cookie容器
        /// </summary>
        public CookieContainer CookieContainer
        {
            get { return _cc; }
            set { _cc = value; }
        }

        public event EventHandler<UploadEventArgs> UploadProgressChanged;
        public event EventHandler<DownloadEventArgs> DownloadProgressChanged;


        /// <summary>
        ///     获取网页源代码
        /// </summary>
        /// <param name="url">网址</param>
        /// <returns></returns>
        public string GetHtml(string url)
        {
            var request = CreateRequest(url, "GET");
            RespHtml = Encoding.GetString(GetData(request));
            return RespHtml;
        }

        /// <summary>
        ///     下载文件
        /// </summary>
        /// <param name="url">文件URL地址</param>
        /// <param name="filename">文件保存完整路径</param>
        public void DownloadFile(string url, string filename)
        {
            FileStream fs = null;
            try
            {
                var request = CreateRequest(url, "GET");
                var data = GetData(request);
                fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                fs.Write(data, 0, data.Length);
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }

        /// <summary>
        ///     从指定URL下载数据
        /// </summary>
        /// <param name="url">网址</param>
        /// <returns></returns>
        public byte[] GetData(string url)
        {
            var request = CreateRequest(url, "GET");
            return GetData(request);
        }

        /// <summary>
        ///     向指定URL发送文本数据
        /// </summary>
        /// <param name="url">网址</param>
        /// <param name="postData">urlencode编码的文本数据</param>
        /// <returns></returns>
        public string Post(string url, string postData)
        {
            var data = Encoding.GetBytes(postData);
            return Post(url, data);
        }

        /// <summary>
        ///     向指定URL发送字节数据
        /// </summary>
        /// <param name="url">网址</param>
        /// <param name="postData">发送的字节数组</param>
        /// <returns></returns>
        public string Post(string url, byte[] postData)
        {
            var request = CreateRequest(url, "POST");
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.KeepAlive = true;
            PostData(request, postData);
            RespHtml = Encoding.GetString(GetData(request));
            return RespHtml;
        }

        /// <summary>
        ///     向指定网址发送mulitpart编码的数据
        /// </summary>
        /// <param name="url">网址</param>
        /// <param name="mulitpartForm">mulitpart form data</param>
        /// <returns></returns>
        public string Post(string url, MultipartForm mulitpartForm)
        {
            var request = CreateRequest(url, "POST");
            request.ContentType = mulitpartForm.ContentType;
            request.ContentLength = mulitpartForm.FormData.Length;
            request.KeepAlive = true;
            PostData(request, mulitpartForm.FormData);
            RespHtml = Encoding.GetString(GetData(request));
            return RespHtml;
        }

        /// <summary>
        ///     读取请求返回的数据
        /// </summary>
        /// <param name="request">请求对象</param>
        /// <returns></returns>
        private byte[] GetData(HttpWebRequest request)
        {
            var response = (HttpWebResponse) request.GetResponse();
            var stream = response.GetResponseStream();
            ResponseHeaders = response.Headers;
            //SaveCookiesToDisk();

            var args = new DownloadEventArgs();
            if (ResponseHeaders[HttpResponseHeader.ContentLength] != null)
                args.TotalBytes = Convert.ToInt32(ResponseHeaders[HttpResponseHeader.ContentLength]);

            var ms = new MemoryStream();
            var count = 0;
            var buf = new byte[BufferSize];
            while ((count = stream.Read(buf, 0, buf.Length)) > 0)
            {
                ms.Write(buf, 0, count);
                if (DownloadProgressChanged != null)
                {
                    args.BytesReceived += count;
                    args.ReceivedData = new byte[count];
                    Array.Copy(buf, args.ReceivedData, count);
                    DownloadProgressChanged(this, args);
                }
            }
            stream.Close();
            //解压    
            if (ResponseHeaders[HttpResponseHeader.ContentEncoding] != null)
            {
                var msTemp = new MemoryStream();
                count = 0;
                buf = new byte[100];
                switch (ResponseHeaders[HttpResponseHeader.ContentEncoding].ToLower())
                {
                    case "gzip":
                        var gzip = new GZipStream(ms, CompressionMode.Decompress);
                        while ((count = gzip.Read(buf, 0, buf.Length)) > 0)
                        {
                            msTemp.Write(buf, 0, count);
                        }
                        return msTemp.ToArray();
                    case "deflate":
                        var deflate = new DeflateStream(ms, CompressionMode.Decompress);
                        while ((count = deflate.Read(buf, 0, buf.Length)) > 0)
                        {
                            msTemp.Write(buf, 0, count);
                        }
                        return msTemp.ToArray();
                    default:
                        break;
                }
            }
            return ms.ToArray();
        }

        /// <summary>
        ///     发送请求数据
        /// </summary>
        /// <param name="request">请求对象</param>
        /// <param name="postData">请求发送的字节数组</param>
        private void PostData(HttpWebRequest request, byte[] postData)
        {
            var offset = 0;
            var sendBufferSize = BufferSize;
            var remainBytes = 0;
            var stream = request.GetRequestStream();
            var args = new UploadEventArgs();
            args.TotalBytes = postData.Length;
            while ((remainBytes = postData.Length - offset) > 0)
            {
                if (sendBufferSize > remainBytes) sendBufferSize = remainBytes;
                stream.Write(postData, offset, sendBufferSize);
                offset += sendBufferSize;
                if (UploadProgressChanged != null)
                {
                    args.BytesSent = offset;
                    UploadProgressChanged(this, args);
                }
            }
            stream.Close();
        }

        /// <summary>
        ///     创建HTTP请求
        /// </summary>
        /// <returns></returns>
        private HttpWebRequest CreateRequest(string url, string method)
        {
            var uri = new Uri(url);

            if (uri.Scheme == "https")
                ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;

            // Set a default policy level for the "http:" and "https" schemes.    
            var policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
            HttpWebRequest.DefaultCachePolicy = policy;

            var request = (HttpWebRequest) WebRequest.Create(uri);
            request.AllowAutoRedirect = false;
            request.AllowWriteStreamBuffering = false;
            request.Method = method;
            if (Proxy != null)
                request.Proxy = Proxy;
            request.CookieContainer = _cc;
            foreach (string key in RequestHeaders.Keys)
            {
                request.Headers.Add(key, RequestHeaders[key]);
            }
            RequestHeaders.Clear();
            return request;
        }

        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors errors)
        {
            return true;
        }

        /// <summary>
        ///     将Cookie保存到磁盘
        /// </summary>
        private static void SaveCookiesToDisk()
        {
            var cookieFile = Environment.GetFolderPath(Environment.SpecialFolder.Cookies) + "\\webclient.cookie";
            FileStream fs = null;
            try
            {
                fs = new FileStream(cookieFile, FileMode.Create);
                var formater = new BinaryFormatter();
                formater.Serialize(fs, _cc);
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }

        /// <summary>
        ///     从磁盘加载Cookie
        /// </summary>
        private static void LoadCookiesFromDisk()
        {
            _cc = new CookieContainer();
            var cookieFile = Environment.GetFolderPath(Environment.SpecialFolder.Cookies) + "\\webclient.cookie";
            if (!File.Exists(cookieFile))
                return;
            FileStream fs = null;
            try
            {
                fs = new FileStream(cookieFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                var formater = new BinaryFormatter();
                _cc = (CookieContainer) formater.Deserialize(fs);
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }
    }
}