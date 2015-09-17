using System;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace DoNet.Utility.HttpProc
{
    /// <summary>
    ///     ���ļ����ı����ݽ���Multipart��ʽ�ı���
    /// </summary>
    public class MultipartForm
    {
        private readonly string _boundary;
        private readonly MemoryStream _ms;
        private byte[] _formData;

        /// <summary>
        ///     ʵ����
        /// </summary>
        public MultipartForm()
        {
            _boundary = string.Format("--{0}--", Guid.NewGuid());
            _ms = new MemoryStream();
            StringEncoding = Encoding.Default;
        }

        /// <summary>
        ///     ��ȡ�������ֽ�����
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
        ///     ��ȡ�˱������ݵ�����
        /// </summary>
        public string ContentType
        {
            get { return string.Format("multipart/form-data; boundary={0}", _boundary); }
        }

        /// <summary>
        ///     ��ȡ�����ö��ַ������õı�������
        /// </summary>
        public Encoding StringEncoding { set; get; }

        /// <summary>
        ///     ���һ���ļ�
        /// </summary>
        /// <param name="name">�ļ�������</param>
        /// <param name="filename">�ļ�������·��</param>
        public void AddFlie(string name, string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("������Ӳ����ڵ��ļ���", filename);

            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var fileData = new byte[fs.Length];
                fs.Read(fileData, 0, fileData.Length);
                AddFlie(name, Path.GetFileName(filename), fileData, fileData.Length);
            }
        }

        /// <summary>
        ///     ���һ���ļ�
        /// </summary>
        /// <param name="name">�ļ�������</param>
        /// <param name="filename">�ļ���</param>
        /// <param name="fileData">�ļ�����������</param>
        /// <param name="dataLength">���������ݴ�С</param>
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
        ///     ����ַ���
        /// </summary>
        /// <param name="name">�ı�������</param>
        /// <param name="value">�ı�ֵ</param>
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
        ///     ��ע����ȡ�ļ�����
        /// </summary>
        /// <param name="filename">������չ�����ļ���</param>
        /// <returns>�磺application/stream</returns>
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