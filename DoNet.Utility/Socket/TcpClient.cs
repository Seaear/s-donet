using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DoNet.Utility.Socket
{
    internal class TcpClient
    {
        private static void Main(string[] args)
        {
            var buffer = new byte[1024];
            var ip = IPAddress.Parse("127.0.0.1");
            var ep = new IPEndPoint(ip, 9110);
            var transferSocket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream,
                ProtocolType.Tcp);
            try
            {
                transferSocket.Connect(ep);
                Console.WriteLine("connect success...");
            }
            catch
            {
                Console.WriteLine("connect failure...");
                return;
            }
            try
            {
                var receiveNumber = transferSocket.Receive(buffer);
                Console.WriteLine("server : " + Encoding.UTF8.GetString(buffer, 0, receiveNumber));
                while (true)
                {
                    var input = Console.ReadLine();
                    if (input == null) continue;
                    if (input == "end") break;
                    transferSocket.Send(Encoding.UTF8.GetBytes(input));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                transferSocket.Shutdown(SocketShutdown.Both);
                transferSocket.Close();
            }

            Console.WriteLine("disconnect...");
            Console.ReadKey();
        }
    }
}