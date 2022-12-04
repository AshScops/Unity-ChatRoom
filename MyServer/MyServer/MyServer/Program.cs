using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyServer
{
    class Program
    {
        /// <summary>
        /// 实例化Message
        /// </summary>
        static Message rec_Message = new Message();
        static Socket serverSocket;
        static List<Socket> clientsList;
        static void Main(string[] args)
        {
            StartServer();
            //暂停
            Console.ReadKey();
        }
        /// <summary>
        /// 开启一个Socket
        /// </summary>
        static void StartServer()
        {
            clientsList = new List<Socket>();

            //实例化一个Socket
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //设置IP
            IPAddress ipAdress = IPAddress.Parse("127.0.0.1");

            //设置网络终结点
            IPEndPoint iPEndPoint = new IPEndPoint(ipAdress, 8080);

            //绑定ip和端口号
            serverSocket.Bind(iPEndPoint);

            //等待队列(开始监听端口号)
            serverSocket.Listen(0);

            //异步接受客户端连接
            serverSocket.BeginAccept(AcceptCallBack, serverSocket);

            Console.WriteLine("[开始接受客户端连接]");
        }


        /// <summary>
        /// 开始发送数据到客户端
        /// </summary>
        /// <param name="toClientsocket">用以连接客户端的Socket</param>
        /// <param name="msg">要传递的数据</param>
        static void BeginSendMessagesToClient(Socket toClientsocket, string msg)
        {
            toClientsocket.Send(Message.GetBytes(msg));
        }

        /// <summary>
        /// 开始接收来自客户端的数据
        /// </summary>
        /// <param name="toClientsocket"></param>
        static void BeginReceiveMessages(Socket toClientsocket)
        {
            toClientsocket.BeginReceive(rec_Message.Data, rec_Message.StartIndex, rec_Message.RemindSize, SocketFlags.None, ReceiveCallBack, toClientsocket);
        }

        /// <summary>
        /// 当客户端连接到服务器时执行的回调函数
        /// </summary>
        /// <param name="ar"></param>
        static void AcceptCallBack(IAsyncResult ar)
        {
            //这里获取到的是向客户端收发消息的Socket
            Socket toClientsocket = serverSocket.EndAccept(ar);
            clientsList.Add(toClientsocket);

            Console.WriteLine("[客户端：{0}--已连接。]", toClientsocket.RemoteEndPoint.ToString());

            //开始接收客户端传来的消息
            BeginReceiveMessages(toClientsocket);

            ////继续等待下一个客户端的链接
            serverSocket.BeginAccept(AcceptCallBack, serverSocket);
        }

        /// <summary>
        /// 接收到来自客户端消息的回调函数
        /// </summary>
        /// <param name="ar"></param>
        static void ReceiveCallBack(IAsyncResult ar)
        {
            Socket toClientsocket = null;
            try
            {
                toClientsocket = ar.AsyncState as Socket;
                int count = toClientsocket.EndReceive(ar);
                Console.WriteLine("收到消息字节数 : " + count);

                //客户端退出，关闭连接
                if (count == 0)
                {
                    if (clientsList.Contains(toClientsocket))
                    {
                        Console.WriteLine("[客户端{0}已关闭连接]", toClientsocket.RemoteEndPoint);
                        toClientsocket.Shutdown(SocketShutdown.Both);
                        toClientsocket.Close();
                        clientsList.Remove(toClientsocket);
                    }
                    toClientsocket.Close();
                    return;
                }

                Console.WriteLine("[从客户端：{0}--接收到数据，解析中...]", toClientsocket.RemoteEndPoint);

                rec_Message.AddCount(count);
                //打印来自客户端的消息
                String msg =  rec_Message.ReadMessage();
                
                foreach(var client in clientsList)
                {
                    BeginSendMessagesToClient(client, msg);
                    Console.WriteLine("[已向客户端{0}发送消息：{1}]", client.RemoteEndPoint, msg);
                }

                //继续监听来自客户端的消息
                toClientsocket.BeginReceive(rec_Message.Data, rec_Message.StartIndex, rec_Message.RemindSize, SocketFlags.None, ReceiveCallBack, toClientsocket);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                if (toClientsocket != null)
                {
                    toClientsocket.Close();
                }
            }
            finally
            {

            }

        }


    }
}
