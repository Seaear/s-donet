using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Aliyun.Acs.Cdn.Model.V20141111;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using Aliyun.OSS;

namespace YKFomms.Utility
{
    /// <summary>
    /// oss上传文件
    /// </summary>
    public class OssHelper
    {
        private readonly string _endpoint;
        private readonly string _accessKeyId;
        private readonly string _accessKeySecret;
        private OssClient _client;

        /// <summary>
        /// OssClient对象
        /// </summary>
        public OssClient Clinet
        {
            get
            {
                try
                {
                    return this._client ?? (this._client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret));
                }
                catch (Exception ex)
                {
                    throw new Exception("创建 OssClient 失败，原因是：" + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public OssHelper(string endpoint, string accessKeyId, string accessKeySecret)
        {
            if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(accessKeyId) || string.IsNullOrWhiteSpace(accessKeySecret))
                throw new Exception("初始化 OssClient 的参数不能为空！");

            this._endpoint = endpoint;
            _accessKeyId = string.Empty;
            this._accessKeyId = accessKeyId;
            this._accessKeySecret = accessKeySecret;
        }

        /// <summary>
        /// 列出指定存储空间下其Key以prefix为前缀的文件的摘要信息OssObjectSummary
        /// </summary>
        /// <param name="bucketName">存储空间的名称</param>
        /// <param name="prefix">限定返回的文件必须以此作为前缀</param>  
        public ObjectListing ListObjects(string bucketName, string prefix, out string msg)
        {
            msg = string.Empty;
            try
            {
                var listObjectsRequest = new ListObjectsRequest(bucketName);
                if (!string.IsNullOrWhiteSpace(prefix))
                    listObjectsRequest.Prefix = prefix;
                return Clinet.ListObjects(listObjectsRequest);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 上传一个文件到oss
        /// </summary>
        /// <param name="bucketName">储存空间的名称</param>
        /// <param name="key">oss的路径</param>
        /// <param name="fileToUpload">本地的文件路径</param>
        /// <param name="msg">错误信息</param>
        /// <returns></returns>
        public string UploadObject(string bucketName, string key, string fileToUpload, out string msg)
        {
            msg = string.Empty;
            try
            {
                var result = Clinet.PutObject(bucketName, key, fileToUpload);
                if (result != null && string.IsNullOrWhiteSpace(result.ETag))
                    return result.ETag;
                return null;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 从oss下载一个文件
        /// </summary>
        /// <param name="bucketName">储存空间的名称</param>
        /// <param name="key">oss的路径</param>
        /// <param name="fileToDownload">本地的文件路径</param>
        /// <param name="msg">错误信息</param>
        /// <returns></returns>
        public bool DownloadObject(string bucketName, string key, string fileToDownload, out string msg)
        {
            msg = string.Empty;
            try
            {
                var obj = Clinet.GetObject(bucketName, key);
                using (var requestStream = obj.Content)
                {
                    byte[] buf = new byte[1024];
                    using (var fs = File.Open(fileToDownload, FileMode.OpenOrCreate))
                    {
                        int len;
                        while ((len = requestStream.Read(buf, 0, 1024)) != 0)
                        {
                            fs.Write(buf, 0, len);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 从oss删除一个文件
        /// </summary>
        /// <param name="bucketName">存储空间的名称</param>
        /// <param name="key">oss的路径</param>
        /// <param name="msg">错误信息</param>
        public bool DeleteObject(string bucketName, string key, out string msg)
        {
            msg = string.Empty;
            try
            {
                Clinet.DeleteObject(bucketName, key);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 从oss删除多个文件
        /// </summary>
        /// <param name="bucketName">存储空间的名称</param>
        /// <param name="keys">oss的路径</param>
        /// <param name="msg">错误信息</param>
        public bool DeleteObjects(string bucketName, List<string> keys, out string msg)
        {
            msg = string.Empty;
            try
            {
                var request = new DeleteObjectsRequest(bucketName, keys, false);
                Clinet.DeleteObjects(request);
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 从oss检查文件是否已存在
        /// </summary>
        /// <param name="bucketName">存储空间的名称</param>
        /// <param name="key">oss的路径</param>
        /// <param name="msg">错误信息</param>
        /// <returns></returns>
        public bool DoesObjectExist(string bucketName, string key, out string msg)
        {
            msg = string.Empty;
            try
            {
                return Clinet.DoesObjectExist(bucketName, key);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 获取oss文件的md5值
        /// </summary>
        /// <returns></returns>
        public string GetObjectMd5Value(string bucketName, string key, out string msg)
        {
            msg = string.Empty;
            try
            {
                var result = Clinet.GetObjectMetadata(bucketName, key);
                if (result == null || string.IsNullOrWhiteSpace(result.ETag))
                    throw new Exception("获取oss对象的MD5值失败！");
                return result.ETag;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取oss上传签名
        /// </summary>
        public object GetUploadSignature(string bucketName, string dir, int uploadExpiration, int maxUploadSize, out string msg)
        {
            msg = string.Empty;
            try
            {
                var deadline = DateTime.Now.AddSeconds(uploadExpiration);
                PolicyConditions policyConds = new PolicyConditions();
                policyConds.AddConditionItem("bucket", bucketName);
                policyConds.AddConditionItem(PolicyConditions.CondContentLengthRange, 1, maxUploadSize);
                var postPolicy = Clinet.GeneratePostPolicy(deadline, policyConds);
                if (string.IsNullOrWhiteSpace(postPolicy))
                    throw new Exception("生成oss请求策略失败！");

                var encPolicy = Convert.ToBase64String(Encoding.UTF8.GetBytes(postPolicy));
                var host = BuildOssRequestUrl(_endpoint, bucketName);
                var signature = ComputeSignature(_accessKeySecret, encPolicy);

                return new
                {
                    accessid = _accessKeyId,
                    policy = encPolicy,
                    signature = signature,
                    host = host,
                    dir = dir
                };
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 获取下载链接
        /// 算法参考：https://help.aliyun.com/document_detail/27135.html?spm=5176.8232292.domaindetail.19.IN2iTT
        /// </summary>
        public string GetDownloadUrl(string bucketName, string key, string cdnDomain,
             DateTime expireDate, string privateKey, out string msg)
        {
            msg = string.Empty;
            try
            {
                var ossUri = Clinet.GeneratePresignedUri(bucketName, key, expireDate);
                var cdnUrl = BuildCdnRequestUrl(_endpoint, bucketName, ossUri.AbsoluteUri, cdnDomain);
                var timestamp = (int)(expireDate - new DateTime(1970, 1, 1)).TotalSeconds;
                var rand = new Random().Next();
                var md5Value = GetMd5(string.Format("/{0}-{1}-{2}-{3}-{4}", key, timestamp, rand, 0, privateKey));
                cdnUrl += string.Format("&auth_key={0}-{1}-{2}-{3}", timestamp, rand, 0, md5Value);
                return cdnUrl;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 预热cdn缓存
        /// </summary>
        public bool PurgeObjectCache(string cdnDomain, string key, out string requestId,
            out string taskIdOrMsg, string objectType = "File", string regionId = "cn-hangzhou")
        {
            try
            {
                var request = new PurgeObjectCachesRequest()
                {
                    DomainName = RemoveHttpString(cdnDomain),
                    ObjectPath = key,
                    ObjectType = objectType,
                };
                var profile = DefaultProfile.GetProfile(regionId, _accessKeyId, _accessKeySecret);
                var client = new DefaultAcsClient(profile);
                var response = client.DoAction(request);
                var xmlTxt = ConvertToString(response.Content);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlTxt);
                if (response.Status == 200)
                {
                    var firstNode = xmlDoc.SelectSingleNode("PurgeObjectCachesResponse");
                    requestId = firstNode.SelectSingleNode("RequestId").InnerText;
                    taskIdOrMsg = firstNode.SelectSingleNode("RefreshTaskId").InnerText;
                    return true;
                }
                else
                {
                    var firstNode = xmlDoc.SelectSingleNode("Error");
                    requestId = firstNode.SelectSingleNode("RequestId").InnerText;
                    taskIdOrMsg = string.Format("{0} : {1}",
                        firstNode.SelectSingleNode("Code").InnerText,
                        firstNode.SelectSingleNode("Message").InnerText);
                    return false;
                }
            }
            catch (Exception ex)
            {
                requestId = string.Empty;
                taskIdOrMsg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 刷新cdn缓存
        /// </summary>
        public bool RefreshObjectCache(string cdnDomain, string key, out string requestId,
            out string taskIdOrMsg, string objectType = "File", string regionId = "cn-hangzhou")
        {
            try
            {
                var request = new RefreshObjectCachesRequest()
                {
                    ObjectPath = string.Format("{0}/{1}", cdnDomain, key),
                    ObjectType = objectType,
                };
                var profile = DefaultProfile.GetProfile(regionId, _accessKeyId, _accessKeySecret);
                var client = new DefaultAcsClient(profile);
                var response = client.DoAction(request);
                var xmlTxt = ConvertToString(response.Content);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlTxt);
                if (response.Status == 200)
                {
                    var firstNode = xmlDoc.SelectSingleNode("RefreshObjectCachesResponse");
                    requestId = firstNode.SelectSingleNode("RequestId").InnerText;
                    taskIdOrMsg = firstNode.SelectSingleNode("RefreshTaskId").InnerText;
                    return true;
                }
                else
                {
                    var firstNode = xmlDoc.SelectSingleNode("Error");
                    requestId = firstNode.SelectSingleNode("RequestId").InnerText;
                    taskIdOrMsg = string.Format("{0} : {1}",
                        firstNode.SelectSingleNode("Code").InnerText,
                        firstNode.SelectSingleNode("Message").InnerText);
                    return false;
                }
            }
            catch (Exception ex)
            {
                requestId = string.Empty;
                taskIdOrMsg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 查询预热、刷新状态，是否在全网生效。
        /// </summary>
        public bool DescribeRefreshTask(string taskId, out string statusOrMsg,
            string regionId = "cn-hangzhou")
        {
            try
            {
                var request = new DescribeRefreshTasksRequest() { TaskId = taskId };
                var profile = DefaultProfile.GetProfile(regionId, _accessKeyId, _accessKeySecret);
                var client = new DefaultAcsClient(profile);
                var response = client.DoAction(request);
                var xmlTxt = ConvertToString(response.Content);
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlTxt);
                if (response.Status == 200)
                {
                    var firstNode = xmlDoc.SelectSingleNode("DescribeRefreshTasksResponse");
                    //requestId = firstNode.SelectSingleNode("RequestId").InnerText;
                    statusOrMsg = firstNode.SelectSingleNode("Tasks/CDNTask/Status").InnerText;
                    return true;
                }
                else
                {
                    var firstNode = xmlDoc.SelectSingleNode("Error");
                    //requestId = firstNode.SelectSingleNode("RequestId").InnerText;
                    statusOrMsg = string.Format("{0} : {1}",
                        firstNode.SelectSingleNode("Code").InnerText,
                        firstNode.SelectSingleNode("Message").InnerText);
                    return false;
                }
            }
            catch (Exception ex)
            {
                //requestId = string.Empty;
                statusOrMsg = ex.Message;
                return false;
            }
        }

        #region 静态方法

        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <returns>32位小写MD5值</returns>
        public static string GetMd5(string value)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] buffer = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
            StringBuilder sb = new StringBuilder(40);
            foreach (byte t in buffer) sb.Append(t.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <returns>32位小写MD5值</returns>
        public static string GetMd5(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] buffer = md5.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder(40);
            foreach (byte t in buffer) sb.Append(t.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// 获取所下载文件的大小
        /// </summary>
        public static long GetContentLength(string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                using (WebResponse response = (WebResponse)request.GetResponse())
                {
                    return response.ContentLength;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 断点续传，下载文件
        /// </summary>
        public static void BreakpointDownloadFile(string url, string filePath)
        {
            //获取所下载文件的大小
            var fileLength = GetContentLength(url);
            if (fileLength <= 0)
                throw new Exception(string.Format("无法获取指定文件{0}的大小", url));

            if (File.Exists(filePath))
            {
                var len = new FileInfo(filePath).Length;
                if (len > fileLength)
                    File.Delete(filePath);
                else if (len == fileLength)
                    return;
            }

            using (var fs = File.Open(filePath, FileMode.OpenOrCreate))
            {
                //开始下载的位置
                var startPosition = fs.Length;

                //移动文件流中的当前指针
                fs.Seek(startPosition, SeekOrigin.Current);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                //设置开始下载的位置
                if (startPosition > 0)
                    request.AddRange(startPosition);

                //向服务器请求，获得服务器回应数据流 
                using (var response = request.GetResponse().GetResponseStream())
                {
                    if (response == null)
                        throw new Exception(string.Format("下载指定文件{0}失败。", url));

                    var len = 40960;
                    byte[] buffer = new byte[len];
                    var readSize = response.Read(buffer, 0, len);
                    while (readSize > 0)
                    {
                        fs.Write(buffer, 0, readSize);
                        readSize = response.Read(buffer, 0, len);
                    }
                }
            }
        }

        #endregion

        #region 私有方法

        private string BuildOssRequestUrl(string endpoint, string bucketName)
        {
            var requestUriBuilder = new StringBuilder();
            if (endpoint.ToLower().StartsWith("http://"))
            {
                requestUriBuilder.Append("http://");
                requestUriBuilder.Append(bucketName);
                requestUriBuilder.Append('.');
                requestUriBuilder.Append(endpoint.Substring("http://".Length));
            }
            else if (endpoint.ToLower().StartsWith("https://"))
            {
                requestUriBuilder.Append("https://");
                requestUriBuilder.Append(bucketName);
                requestUriBuilder.Append('.');
                requestUriBuilder.Append(endpoint.Substring("https://".Length));
            }
            else
            {
                requestUriBuilder.Append("http://");
                requestUriBuilder.Append(bucketName);
                requestUriBuilder.Append('.');
                requestUriBuilder.Append(endpoint);
            }
            return requestUriBuilder.ToString();
        }

        private string BuildCdnRequestUrl(string endpoint, string bucketName, string ossRequestUrl, string cdnDomain)
        {
            var length = bucketName.Length + endpoint.Length + 1;
            return string.Format("{0}{1}", cdnDomain, ossRequestUrl.Substring(length));
        }

        private string ComputeSignature(string key, string data)
        {
            using (var algorithm = KeyedHashAlgorithm.Create("HmacSHA1".ToUpperInvariant()))
            {
                algorithm.Key = Encoding.UTF8.GetBytes(key.ToCharArray());
                return Convert.ToBase64String(
                    algorithm.ComputeHash(Encoding.UTF8.GetBytes(data.ToCharArray())));
            }
        }

        /// <summary>
        /// 二进制转字符串
        /// </summary>
        private string ConvertToString(byte[] thebyte)
        {
            char[] chars = new char[Encoding.Default.GetCharCount(thebyte, 0, thebyte.Length)];
            Encoding.Default.GetChars(thebyte, 0, thebyte.Length, chars, 0);
            string newString = new string(chars);
            return newString;
        }

        /// <summary>
        /// 字符串转二进制
        /// </summary>
        private byte[] ConvertToByte(string theString)
        {
            byte[] byteStream = Encoding.Default.GetBytes(theString);
            return byteStream;
        }

        /// <summary>
        /// 移除http前缀
        /// </summary>
        private string RemoveHttpString(string url)
        {
            if (url.ToLower().StartsWith("http://"))
                return url.Substring(7);
            if (url.ToLower().StartsWith("https://"))
                return url.Substring(8);
            return url;
        }

        #endregion

    }
}
