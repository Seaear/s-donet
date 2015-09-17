using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DoNet.Utility.Socket
{
    internal class TcpServer
    {
        private static readonly byte[] buffer = new byte[1024];

        private static void Main(string[] args)
        {
            try
            {
                var ip = IPAddress.Parse("127.0.0.1");
                var ep = new IPEndPoint(ip, 9110);
                var listenSocket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream,
                    ProtocolType.Tcp);
                listenSocket.Bind(ep);
                listenSocket.Listen(10);
                Console.WriteLine("start to listen...");
                var thread = new Thread(ListenClientConnect);
                thread.Start(listenSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }

        private static void ListenClientConnect(object listenSocket)
        {
            var myClientSocket = (System.Net.Sockets.Socket) listenSocket;
            while (true)
            {
                // 等待连接
                var transferSocket = myClientSocket.Accept();
                transferSocket.Send(Encoding.UTF8.GetBytes("hey guy..."));
                var thread = new Thread(ReceiveMessage);
                thread.Start(transferSocket);
            }
        }

        private static void ReceiveMessage(object transferSocket)
        {
            var myClientSocket = (System.Net.Sockets.Socket) transferSocket;
            while (true)
            {
                try
                {
                    // 等待接受数据
                    var receiveNumber = myClientSocket.Receive(buffer);
                    if (receiveNumber == 0)
                    {
                        Console.WriteLine("client " + myClientSocket.RemoteEndPoint + " : disconnect...");
                        myClientSocket.Shutdown(SocketShutdown.Both);
                        myClientSocket.Close();
                        break;
                    }
                    Console.WriteLine("client " + myClientSocket.RemoteEndPoint + " : " +
                                      Encoding.UTF8.GetString(buffer, 0, receiveNumber));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    myClientSocket.Shutdown(SocketShutdown.Both);
                    myClientSocket.Close();
                }
            }
        }
    }
}