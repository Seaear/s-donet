using System;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace DoNet.Utility.HttpProc
{
    /// <summary>
    ///     对文件和文本数据进行Multipart形式的编码
    /// </summary>
    public class MultipartForm
    {
        private readonly string _boundary;
        private readonly MemoryStream _ms;
        private byte[] _formData;

        /// <summary>
        ///     实例化
        /// </summary>
        public MultipartForm()
        {
            _boundary = string.Format("--{0}--", Guid.NewGuid());
            _ms = new MemoryStream();
            StringEncoding = Encoding.Default;
        }

        /// <summary>
        ///     获取编码后的字节数组
        /// </summary>
        public byte[] FormData
        {
            get
            {
                if (_formData == null)
                {
                    var buffer = StringEncoding.GetBytes("--" + _boundary + "--\r\n");
                    _ms.Write(buffer, 0, buffer.Length);
                    _formData = _ms.ToArray();
                }
                return _formData;
            }
        }

        /// <summary>
        ///     获取此编码内容的类型
        /// </summary>
        public string ContentType
        {
            get { return string.Format("multipart/form-data; boundary={0}", _boundary); }
        }

        /// <summary>
        ///     获取或设置对字符串采用的编码类型
        /// </summary>
        public Encoding StringEncoding { set; get; }

        /// <summary>
        ///     添加一个文件
        /// </summary>
        /// <param name="name">文件域名称</param>
        /// <param name="filename">文件的完整路径</param>
        public void AddFlie(string name, string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("尝试添加不存在的文件。", filename);

            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var fileData = new byte[fs.Length];
                fs.Read(fileData, 0, fileData.Length);
                AddFlie(name, Path.GetFileName(filename), fileData, fileData.Length);
            }
        }

        /// <summary>
        ///     添加一个文件
        /// </summary>
        /// <param name="name">文件域名称</param>
        /// <param name="filename">文件名</param>
        /// <param name="fileData">文件二进制数据</param>
        /// <param name="dataLength">二进制数据大小</param>
        public void AddFlie(string name, string filename, byte[] fileData, int dataLength)
        {
            if (dataLength <= 0 || dataLength > fileData.Length)
            {
                dataLength = fileData.Length;
            }
            var sb = new StringBuilder();
            sb.AppendFormat("--{0}\r\n", _boundary);
            sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\";filename=\"{1}\"\r\n", name, filename);
            sb.AppendFormat("Content-Type: {0}\r\n", GetContentType(filename));
            sb.Append("\r\n");
            var buf = StringEncoding.GetBytes(sb.ToString());
            _ms.Write(buf, 0, buf.Length);
            _ms.Write(fileData, 0, dataLength);
            var crlf = StringEncoding.GetBytes("\r\n");
            _ms.Write(crlf, 0, crlf.Length);
        }

        /// <summary>
        ///     添加字符串
        /// </summary>
        /// <param name="name">文本域名称</param>
        /// <param name="value">文本值</param>
        public void AddString(string name, string value)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("--{0}\r\n", _boundary);
            sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\"\r\n", name);
            sb.Append("\r\n");
            sb.AppendFormat("{0}\r\n", value);
            var buf = StringEncoding.GetBytes(sb.ToString());
            _ms.Write(buf, 0, buf.Length);
        }

        /// <summary>
        ///     从注册表获取文件类型
        /// </summary>
        /// <param name="filename">包含扩展名的文件名</param>
        /// <returns>如：application/stream</returns>
        private string GetContentType(string filename)
        {
            var contentType = "application/stream";
            if (filename == null)
                return contentType;
            using (var fileExtKey = Registry.ClassesRoot.OpenSubKey(Path.GetExtension(filename)))
            {
                if (fileExtKey != null)
                    contentType = fileExtKey.GetValue("Content Type", contentType).ToString();
            }
            return contentType;
        }
    }
}