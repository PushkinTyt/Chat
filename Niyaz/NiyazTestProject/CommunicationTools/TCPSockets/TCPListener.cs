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
    public class TCPListener
    {
        TcpListener listener;
        //bool shouldWork = true;
        string adress;

        Thread acceptThread;
        List<Thread> threads = new List<Thread>();

        Dictionary<EndPoint, Socket> clients = new Dictionary<EndPoint, Socket>();

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

        //Обработка сообщений
        //Описываем сигнатуру метода-обработчика 
        public delegate void ReceiveMethod(IPEndPoint endpoint, MetaData md, string msg);
        //Событие, на которое будут подписываться
        public event ReceiveMethod onMessage;


        //Обработка ошибок
        public delegate void ErrorHandler(SocketException ex, Socket socket);
        public event ErrorHandler onError;

        public TCPListener(int port)
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

        public TCPListener(string ip, int port)
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
            while(true)
            {
                if(listener.Pending())
                {
                    try
                    {
                        Socket client = listener.AcceptSocket();
                        clients.Add(client.RemoteEndPoint, client);
                        Thread clientHandler = new Thread(() => listenClient(client));
                        clientHandler.Start();

                        threads.Add(clientHandler);
                    }
                    catch(Exception ex)
                    {
                        Debug.Print(ex.Message);
                    }

                }
            }
        }

        /// <summary>
        /// Принимает сообщение от клиентов. При получении сообщения генерируется событие onMessage
        /// </summary>
        /// <param name="client">Обрабатываемый клиент</param>
        private void listenClient(Socket client)
        {
            int bufferSize = MetaData.defaultPacketSize;
            IPEndPoint endPoint = (IPEndPoint)client.RemoteEndPoint;

            while (true)
            {
                string msg = "";

                byte[] buffer = new byte[bufferSize];

                try
                {
                    client.Receive(buffer);
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

                if(md.MessageSize > 0)
                {
                    buffer = new byte[md.MessageSize];
                    try
                    {
                        client.Receive(buffer);
                    }
                    catch (SocketException ex)
                    {
                        Debug.Print(ex.Message);
                        break;
                    }

                    msg += md.Encoding.GetString(buffer);

                    msg = msg.TrimEnd('\0'); //В UTF-8 в конце строки куча \0, консоли это не нравится
                }

                onMessage(endPoint, md, msg);
            }
        }

        /// <summary>
        /// Отправка сообщения клиенту
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="msg">Пересылаемое сообщение</param>
        public bool Send(IPEndPoint endpoint, string msg, MetaData md)
        {
            Socket client = clients[endpoint];

            if (client != null)
            {
                if (client.Connected)
                {
                    byte[] buffer;

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
                    try
                    {
                        client.Send(buffer);
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.Message);
                        return false;
                    }

                    //Шлем сообщение
                    buffer = new byte[md.MessageSize];
                    msgStream.Position = 0;
                    msgStream.Read(buffer, 0, md.MessageSize);
                    try
                    {
                        client.Send(buffer);
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.Message);
                        return false;
                    }
                }
                else
                {
                    Debug.Print("Client is not connected");
                    return false;
                }
            }
            else
            {
                throw new Exception("Нет такого клиента, смотри, что пихаешь, остолоп!");
            }
            return true;
        }

        /// <summary>
        /// Завершает обработку входящих потоков и разрывает соединение с подключенным клиентами
        /// </summary>
        public void Close()
        {
            acceptThread.Abort();
            clients.ToList().ForEach(x => x.Value.Close());

            listener.Stop();

            threads.ForEach(x => x.Abort());
        }

        ~TCPListener()
        {
            this.Close();
        }
    }
}
