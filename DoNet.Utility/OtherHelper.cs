using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DoNet.Utility
{
    internal class OtherHelper
    {
        /// <summary>
        ///     序列化
        /// </summary>
        /// <param name="data">要序列化的对象</param>
        /// <returns>返回存放序列化后的数据缓冲区</returns>
        public static byte[] Serialize(object data)
        {
            var formatter = new BinaryFormatter();
            var rems = new MemoryStream();
            formatter.Serialize(rems, data);
            return rems.GetBuffer();
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <param name="data">数据缓冲区</param>
        /// <returns>对象</returns>
        public static object Deserialize(byte[] data)
        {
            var formatter = new BinaryFormatter();
            var rems = new MemoryStream(data);
            return formatter.Deserialize(rems);
        }
    }
}