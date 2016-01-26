using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace CommunicationTools
{
    public class TCPClient
    {
        TcpClient client;
        //bool shouldWork = true;

        Thread messageHandler;
        Thread pingThread;

        //Описываем сигнатуру метода-обработчика
        public delegate void ReceiveMethod(MetaData md, string msg);
        //Событие, на которое будут подписываться
        public event ReceiveMethod onMessage;

        //Событие дисконнекта
        public delegate void DisconnectHandler();
        public event DisconnectHandler onDisconnect;


        public TCPClient(string ip, int port)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            client = new TcpClient();

            try
            {
                client.Connect(endpoint);
                pingThread = new Thread(ping);
                pingThread.Start();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void Reconnect(string ip, int port)
        {
            this.Close();

            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            client = new TcpClient();

            try
            {
                client.Connect(endpoint);
                pingThread = new Thread(ping);
                pingThread.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Начинает прослушивание сообщений, пришедших от сервера
        /// </summary>
        public void StartListen()
        {
            messageHandler = new Thread(ReceiveData);
            messageHandler.Start();
        }

        void ping()
        {
            MetaData pingPackage = new MetaData(MetaData.Roles.none, MetaData.Actions.ping);
            try
            {
                while (true)
                {
                    this.Send("", pingPackage);
                    Thread.Sleep(300);
                }
            }
            catch(ThreadAbortException)
            {
                return;
            }
            catch(SocketException)
            {
                if(onDisconnect != null)
                {
                    onDisconnect();
                }
                return;
            }
        }

        public void Send(string msg, MetaData md)
        {
            NetworkStream socketStream = client.GetStream();

            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, md);

            byte[] msgBytes = md.Encoding.GetBytes(msg);
            ms.Write(msgBytes, 0, msgBytes.Length);

            byte[] generalMsg = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(generalMsg, 0, (int)ms.Length);

            socketStream.Write(generalMsg, 0, generalMsg.Length);
        }

        /// <summary>
        /// Принимает сообщения от сервера и генерирует соответсвующее событие
        /// </summary>
        public void ReceiveData()
        {
            NetworkStream networkStream = client.GetStream();

            while (true)
            {
                try
                {
                    if (client.Available > 0)
                    {
                        BinaryFormatter formatter = new BinaryFormatter();

                        MetaData md = (MetaData)formatter.Deserialize(networkStream);

                        string msg = "";
                        int bufSize = 512;
                        byte[] msgBytes = new byte[bufSize];

                        if (md.MessageSize > 0)
                        {
                            networkStream.Read(msgBytes, 0, md.MessageSize);
                            msg += md.Encoding.GetString(msgBytes);
                        }

                        msg = msg.TrimEnd('\0');
                        onMessage(md, msg);
                    }
                    Thread.Sleep(500);
                }
                catch(Exception ex)
                {
                    Debug.Print(ex.Message);
                    if(client != null)
                    {
                        client.Client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }
                    break;
                }
            }
        }

        public string ReceiveSyncData(int timeOut)
        {
            NetworkStream networkStream = client.GetStream();
            client.ReceiveTimeout = timeOut;
            BinaryFormatter formatter = new BinaryFormatter();

            MetaData md = (MetaData)formatter.Deserialize(networkStream);

            string msg = "";

            if (md.MessageSize > 0)
            {
                byte[] msgBytes = new byte[md.MessageSize];
                networkStream.Read(msgBytes, 0, md.MessageSize);
                msg += md.Encoding.GetString(msgBytes);
            }

            msg = msg.TrimEnd('\0');
            client.ReceiveTimeout = 0;
            return msg;
        }
        /// <summary>
        /// Разрывает подключение с сервером
        /// </summary>
        public void Close()
        {
            if(client != null)
            {
                client.Client.Shutdown(SocketShutdown.Both);
                client.Close();
                if(messageHandler != null)
                {
                    messageHandler.Abort();
                }
                
                if(pingThread != null)
                {
                    pingThread.Abort();
                }
            }
        }

        ~TCPClient()
        {
            this.Close();
            Debug.Print("TCPClient closed");
        }
    }
}
