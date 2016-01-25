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

        Dictionary<IPEndPoint, TcpClient> clients = new Dictionary<IPEndPoint, TcpClient>();

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
        public delegate void ErrorHandler(SocketException ex, TCPListener tcpListener);
        public event ErrorHandler onError;

        public TCPListener(int port)
        {
            IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

            IPAddress compIP = adresses.First(x => x.AddressFamily == AddressFamily.InterNetwork);
            IPEndPoint endPoint = new IPEndPoint(compIP, port);
            adress = endPoint.ToString();
            try
            {
                listener = new TcpListener(endPoint);
            }
            catch(Exception ex)
            {
                throw ex;
            }
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
            while (true)
            {
                try
                {
                    if (listener.Pending())
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        clients.Add((IPEndPoint)client.Client.RemoteEndPoint, client);
                        Thread clientHandler = new Thread(() => listenClient(client));
                        clientHandler.Start();

                        threads.Add(clientHandler);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    break;
                }
                Thread.Sleep(300);
            }
        }

        /// <summary>
        /// Принимает сообщение от клиентов. При получении сообщения генерируется событие onMessage
        /// </summary>
        /// <param name="client">Обрабатываемый клиент</param>
        private void listenClient(TcpClient client)
        {
            int bufferSize = MetaData.defaultPackageSize;
            IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            NetworkStream networkStream = client.GetStream();

            while (true)
            {
                try
                {
                    if (client.Available > 0)
                    {
                        BinaryFormatter formatter = new BinaryFormatter();

                        MetaData md = (MetaData)formatter.Deserialize(networkStream);

                        if(md.Action != MetaData.Actions.ping)
                        {
                            string msg = "";
                            int bufSize = 512;
                            byte[] msgBytes = new byte[bufSize];

                            if (md.MessageSize > 0)
                            {
                                networkStream.Read(msgBytes, 0, md.MessageSize);
                                msg = md.Encoding.GetString(msgBytes);

                                msg = msg.TrimEnd('\0');
                            }

                            onMessage(endPoint, md, msg);
                        }
                    }
                    Thread.Sleep(500);
                }
                catch (SocketException ex)
                {
                    Debug.Print(ex.Message);
                    clients.Remove((IPEndPoint)client.Client.RemoteEndPoint);
                    client.Client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    threads.Remove(Thread.CurrentThread);
                    break;
                }
            }
        }

        /// <summary>
        /// Отправка сообщения клиенту
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="msg">Пересылаемое сообщение</param>
        public void Send(IPEndPoint endpoint, string msg, MetaData md)
        {
            TcpClient client = clients[endpoint];

            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, md);

            byte[] msgBytes = md.Encoding.GetBytes(msg);
            ms.Write(msgBytes, 0, msgBytes.Length);

            byte[] generalMsg = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(generalMsg, 0, (int)ms.Length);

            NetworkStream socketStream = client.GetStream();
            socketStream.Write(generalMsg, 0, generalMsg.Length);
        }

        /// <summary>
        /// Завершает обработку входящих потоков и разрывает соединение с подключенным клиентами
        /// </summary>
        public void Close()
        {
            acceptThread.Abort();
            clients.ToList().ForEach(x =>
                    {
                        x.Value.Client.Shutdown(SocketShutdown.Both);
                        x.Value.Close();
                    });

            listener.Stop();

            threads.ForEach(x => x.Abort());
        }

        ~TCPListener()
        {
            this.Close();
        }
    }
}
