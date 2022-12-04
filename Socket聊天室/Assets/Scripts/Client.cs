using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;
using FairyGUI;
using System.Collections.Generic;
using System.Collections;

namespace MyClient
{
    public class Client : MonoBehaviour
    {
        /// <summary>
        /// ʵ����Message
        /// </summary>
        private Message rec_Message = new Message();
        //�����ͻ���
        private Socket clientSocket;
        private String clientName = "";

        public GameObject chatRoom;
        public GameObject enterRoom;

        private GComponent chatRoomRoot;
        private GComponent enterRoomRoot;

        private List<String> messages;

        private void Awake()
        {
            messages = new List<string>();
        }

        private void Start()
        {
            chatRoomRoot = chatRoom.GetComponent<UIPanel>().ui;
            enterRoomRoot = enterRoom.GetComponent<UIPanel>().ui;

            GObject sendBtn = chatRoomRoot.GetChild("n1").asButton;
            sendBtn.onClick.Add(SendButtonClicked);

            GObject closeBtn = chatRoomRoot.GetChild("n3").asButton;
            closeBtn.onClick.Add(CloseButtonClicked);

            GObject enterBtn = enterRoomRoot.GetChild("n4").asButton;
            enterBtn.onClick.Add(EnterButtonClicked);

            chatRoom.SetActive(false);
            enterRoom.SetActive(true);
        }

        private void Update()
        {
            if (messages.Count != 0)
            {
                ShowInChatList(messages[0]);
                messages.Remove(messages[0]);
            }
        }

        /// <summary>
        /// �����ͻ��˲����ӵ���������
        /// </summary>
        private void StartClient()
        {
            this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
                Debug.Log("���ӷ������ɹ�");
            }
            catch (Exception e)
            {
                Debug.Log(e);
                Debug.Log("���ӷ�����ʧ��");
            }

            String msg = clientName + "����Ƶ��";
            BeginSendMessagesToServer(msg);

            BeginReceiveMessages();
        }

        /// <summary>
        /// ��ʼ�������ݵ������
        /// </summary>
        /// <param name="msg">Ҫ���ݵ�����</param>
        private void BeginSendMessagesToServer(string msg)
        {
            try
            {
                clientSocket.Send(Message.GetBytes(msg));
                
                Debug.Log("���ͳɹ�!");
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        /// <summary>
        /// ��ʼ�������Է���˵�����
        /// </summary>
        /// <param name="toServersocket"></param>
        private void BeginReceiveMessages()
        {
            clientSocket.BeginReceive(rec_Message.Data, rec_Message.StartIndex, rec_Message.RemindSize, SocketFlags.None, ReceiveCallBack, null);
        }

        /// <summary>
        /// ���յ����Է������Ϣ�Ļص�����
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                if (!clientSocket.Connected) return;

                int count = clientSocket.EndReceive(ar);
                Debug.Log("�յ���Ϣ�ֽ��� : "+ count);

                rec_Message.AddCount(count);

                //��ʾ����˵���Ϣ
                String msg = rec_Message.ReadMessage();

                messages.Add(msg);

                //�����������Է���˵���Ϣ
                clientSocket.BeginReceive(rec_Message.Data, rec_Message.StartIndex, rec_Message.RemindSize, SocketFlags.None, ReceiveCallBack, null);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private void ShowInChatList(String msg)
        {
            //��ʾ���������
            GList chatList = chatRoomRoot.GetChild("n2").asList;
            GComponent gf = UIPackage.CreateObject("MyFGUI" , "MyChatBubble").asCom;
            GTextField text = gf.GetChild("n0").asTextField;
            text.text = msg;
            chatList.AddChild(gf);

            if (chatList.numChildren > 0)
            {
                chatList.ScrollToView(chatList.numChildren - 1);//�������ײ�
            }

            Debug.Log("ShowInChatListDone");
        }

        public void EnterButtonClicked()
        {
            Debug.Log("EnterButtonClicked");

            GTextField gtf = enterRoomRoot.GetChild("n1").asTextField;

            if (!gtf.text.Equals(""))
            {
                clientName = gtf.text;
                StartClient();

                chatRoom.SetActive(true);
                enterRoom.SetActive(false);
            }
        }

        public void SendButtonClicked()
        {
            Debug.Log("SendButtonClicked");

            GTextField gtf = chatRoomRoot.GetChild("n0").asTextField;
            
            if( ! gtf.text.Equals(""))
            {
                BeginSendMessagesToServer(clientName + " : " + gtf.text);
                gtf.text = "";
            }
        }

        public void CloseButtonClicked()
        {
            Debug.Log("CloseButtonClicked");

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();

            enterRoom.SetActive(true);
            chatRoom.SetActive(false);
        }
    }
}