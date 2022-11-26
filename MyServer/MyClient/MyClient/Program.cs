using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace MyClient
{
    class Program
    {
        /// <summary>
        /// 实例化Message
        /// </summary>
        static Message rec_Message = new Message();
        //声明客户端
        static Socket clientSocket;

        static void Main(string[] args)
        {
            StartClient();
            while (true)
            {
                Console.WriteLine("请输入想要向服务器发送的字符串：");
                string data = Console.ReadLine();
                BeginSendMessagesToServer(data);

                if (!clientSocket.Connected) break;
            }
        }
        /// <summary>
        /// 开启客户端并连接到服务器端
        /// </summary>
        static void StartClient()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
                Console.WriteLine("连接服务器成功");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("连接服务器失败");
            }

            String msg = "客户端" +  clientSocket.RemoteEndPoint.ToString() + "加入频道";
            BeginSendMessagesToServer(msg);
            BeginReceiveMessages();
        }

        /// <summary>
        /// 开始发送数据到服务端
        /// </summary>
        /// <param name="msg">要传递的数据</param>
        static void BeginSendMessagesToServer(string msg)
        {
            try
            {
                if (msg.Equals("/close"))
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    //clientSocket.Disconnect(true);
                    return;
                }

                clientSocket.Send(Message.GetBytes(msg));
                //Console.WriteLine("{0} 发送成功!", msg);
                Console.WriteLine("发送成功!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// 开始接收来自服务端的数据
        /// </summary>
        /// <param name="toServersocket"></param>
        static void BeginReceiveMessages()
        {
            clientSocket.BeginReceive(rec_Message.Data, rec_Message.StartIndex, rec_Message.RemindSize, SocketFlags.None, ReceiveCallBack, null);
        }

        /// <summary>
        /// 接收到来自服务端消息的回调函数
        /// </summary>
        /// <param name="ar"></param>
        static void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                if (!clientSocket.Connected) return;

                int count = clientSocket.EndReceive(ar);
                //Console.WriteLine("从服务端接收到数据,解析中。。。");

                rec_Message.AddCount(count);

                //打印来自服务端的消息
                rec_Message.ReadMessage();

                //继续监听来自服务端的消息
                clientSocket.BeginReceive(rec_Message.Data, rec_Message.StartIndex, rec_Message.RemindSize, SocketFlags.None, ReceiveCallBack, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}
