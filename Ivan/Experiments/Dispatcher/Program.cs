using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;
using CommunicationTools;

namespace Dispatcher
{
    class Program
    {
        static Dispatcher dispatcher;
        //private Connection CacheServerCon;  // соединение с кэш-сервером

        static void Main(string[] args)
        {
            dispatcher = new Dispatcher();
            Console.ReadKey();
        }

        
    }
    

    /// <summary>
    /// Соединение по сокету
    /// </summary>
    public class Connection
    {
        public Socket clientSocket;
        private NetworkStream socketStream;
        private BinaryReader reader;
        private BinaryWriter writer;
        public string ip_server;

        public Connection(Socket NewClient)
        {
            clientSocket = NewClient;
            socketStream = new NetworkStream(NewClient);
            reader = new BinaryReader(socketStream);
            writer = new BinaryWriter(socketStream);
            ip_server = ((IPEndPoint)NewClient.LocalEndPoint).Address.ToString();

            Thread startListen = new Thread(new ThreadStart(Receive));
            startListen.Start();
        }

        public void Send(string message)
        {
            try
            {
                writer.Write(message);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.InnerException);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
            }
        }

        public void Receive()
        {
            try
            {
                string reply = "";
                do
                {
                    try
                    {
                        //byte[] msg = new byte[15];
                        //int res = clientSocket.Receive(msg);
                        Debug.Print(String.Format("connection: {0}    availiable: {1}", clientSocket.Connected.ToString(), clientSocket.Available.ToString()) );
                        byte[] r = reader.ReadBytes(10);
                        // do something
                        
                        string msgStr = Encoding.Unicode.GetString(r);
                        Console.WriteLine(msgStr);
                    }
                    catch (Exception e)
                    {
                        SocketException ee = (SocketException)e.InnerException;
                        if (ee.ErrorCode == 10053) //обрабатываем исключение при выключении сервера
                        {
                            // event for update server table and delete this connection
                            return;
                        }
                    }
                } while (clientSocket.Connected);
            }
            catch (Exception)
            {
            }
            
        }

        public void Stop()
        {
            writer.Close();
            reader.Close();
            socketStream.Close();
            clientSocket.Close();
        }
    }





    /// <summary>
    /// держит подключения с серверами(кэш/реферирования)
    /// </summary>
    public class ServersComunicator
    {
        private TcpListener listener;
        private Thread startListen;
        public List<Connection> referatorServers = new List<Connection>();

        int port;

        public ServersComunicator(int _port)
        {
            port = _port;
        }

        public void SendToAll(string str)
        {
            foreach (Connection c in referatorServers)
                c.Send(str);

            // add cache server
        }

        public void Start()
        {
            startListen = new Thread(new ThreadStart(Listen));
            startListen.Start();
        }

        public void Stop()
        {
            try
            {
                listener.Stop();
                foreach (Connection c in referatorServers)
                {
                    c.Stop();
                }
                referatorServers.Clear();
            }
            catch
            {
            }
        }

        //удаляем клиента из листа клиентов
        public void RemoveClient(Socket sock)
        {
            Connection buf = null;
            foreach (Connection c in referatorServers)
            {
                if (c.clientSocket == sock)
                    buf = c;
            }
            referatorServers.Remove(buf);
        }

        //сервер начинает слушать и подключать клиентов
        public void Listen()
        {
            try
            {
                try
                {
                    listener = new TcpListener(port);
                    listener.Start(10);
                }
                catch (Exception e)
                {
                    //не удалось включить сервер
                    return;
                }
                while (true)
                {
                    Socket clientSocket = listener.AcceptSocket();
                    //добавляем клиента
                    Connection Client = new Connection(clientSocket);
                    Console.WriteLine("подключен клиент");
                    referatorServers.Add(Client);
                }
            }
            catch { }
        }
    }
}
