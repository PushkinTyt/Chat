using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;


namespace CommunicationTools
{
    public class TCPSListener
    {
        TcpListener listener;
        bool shouldWork = true;
        string adress;

        Thread acceptThread;
        List<Thread> threads = new List<Thread>();

        Dictionary<EndPoint, TcpClient> clients = new Dictionary<EndPoint, TcpClient>();

        /// <summary>
        /// IP-адрес машины, на которой работает этот сокет
        /// </summary>
        public string Adress
        {
            get
            {
                return adress;
            }
        }

        //Описываем сигнатуру метода-обработчика 
        public delegate void ReceiveMethod(IPEndPoint endpoint, string msg);
        //Событие, на которое будут подписываться
        public event ReceiveMethod onMessage;

        public TCPSListener(int port)
        {
            IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

            IPAddress compIP = adresses.First(x => x.AddressFamily == AddressFamily.InterNetwork);
            IPEndPoint endPoint = new IPEndPoint(compIP, port);
            adress = endPoint.ToString();
            listener = new TcpListener(endPoint);

            listener.Start();
            acceptThread = new Thread(acceptClients);
            acceptThread.Start();
        }

        public TCPSListener(string ip, int port)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            adress = endPoint.ToString();
            listener = new TcpListener(endPoint);
        }

        /// <summary>
        /// Запускает поток обработки входящих подключений
        /// </summary>
        public void StartListen()
        {
            listener.Start();
            acceptThread = new Thread(acceptClients);
            acceptThread.Start();
        }

        /// <summary>
        /// Обработка запросов на подключение
        /// </summary>
        void acceptClients()
        {
            while(shouldWork)
            {
                if(listener.Pending())
                {
                    TcpClient client = listener.AcceptTcpClient();
                    clients.Add(client.Client.RemoteEndPoint, client);
                    Thread clientHandler = new Thread(() => listen(client));
                    clientHandler.Start();

                    threads.Add(clientHandler);
                }
            }
        }

        /// <summary>
        /// Принимает сообщение от клиентов. При получении сообщения генерируется событие onMessage
        /// </summary>
        /// <param name="client">Обрабатываемый клиент</param>
        void listen(TcpClient client)
        {
            int bufferSize = MetaData.defaultPacketSize;

            while (shouldWork)
            {
                string msg = "";

                byte[] buffer = new byte[bufferSize];

                try
                {
                    client.Client.Receive(buffer);
                }
                catch(SocketException ex)
                {
                    Debug.Print(ex.Message);
                    break;
                }

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                ms.Write(buffer, 0, bufferSize);
                ms.Position = 0;

                MetaData md = (MetaData) formatter.Deserialize(ms);

                buffer = new byte[md.BufferSize];
                for (int i=0; i<md.PackageNum; i++)
                {
                    try
                    {
                        client.Client.Receive(buffer);
                    }
                    catch (SocketException ex)
                    {
                        Debug.Print(ex.Message);
                        break;
                    }
                    msg += md.Encoding.GetString(buffer);
                }

                
                msg = msg.TrimEnd('\0'); //В UTF-8 в конце строки куча \0, консоли это не нравится
                onMessage((IPEndPoint)client.Client.RemoteEndPoint, msg);

            }
        }

        /// <summary>
        /// Отправка сообщения клиенту
        /// </summary>
        /// <param name="IP">IP-адрес клиента</param>
        /// <param name="port">Порт клиента</param>
        /// <param name="msg">Пересылаемое сообщение</param>
        public void SendToClient(string IP, int port, string msg)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(IP), port);
            this.SendToClient(endpoint, msg);
        }

        /// <summary>
        /// Отправка сообщения клиенту
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="msg">Пересылаемое сообщение</param>
        public void SendToClient(IPEndPoint endpoint, string msg)
        {
            TcpClient client = clients[endpoint];

            if (client != null)
            {
                if (client.Connected)
                {
                    byte[] buffer;

                    MetaData md = new MetaData(1, MetaData.Roles.Client, MetaData.Actions.none);
                    md.CalcMsgData(msg);
                    //md.Encoding = Encoding.Unicode;

                    MemoryStream msgStream = new MemoryStream();
                    byte[] bytes = md.Encoding.GetBytes(msg);
                    msgStream.Write(md.Encoding.GetBytes(msg), 0, bytes.Length);

                    MemoryStream ms = new MemoryStream();
                    BinaryFormatter formatter = new BinaryFormatter();

                    //Шлем административный пакет
                    formatter.Serialize(ms, md);
                    ms.Position = 0;
                    buffer = new byte[ms.Length];
                    ms.Read(buffer, 0, Convert.ToInt32(ms.Length));
                    client.Client.Send(buffer);

                    //Шлем сообщение
                    buffer = new byte[md.BufferSize];
                    msgStream.Position = 0;
                    int readCount = 0;
                    while (readCount < msgStream.Length)
                    {
                        readCount += msgStream.Read(buffer, 0, md.BufferSize);
                        client.Client.Send(buffer);
                    }
                }
                else
                {
                    throw new Exception("Подключение не удалось");
                }
            }
            else
            {
                throw new Exception("Нет такого клиента, смотри, что пихаешь, остолоп!");
            }
        }

        /// <summary>
        /// Завершает обработку входящих потоков и разрывает соединение с подключенным клиентами
        /// </summary>
        public void Close()
        {
            shouldWork = false;
            acceptThread.Abort();
            clients.ToList().ForEach(x => x.Value.Close());

            listener.Stop();

            threads.ForEach(x => x.Abort());
        }


    }
}
