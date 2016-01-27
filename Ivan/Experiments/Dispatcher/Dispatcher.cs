using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationTools;
using System.Configuration;
using System.Net;
using System.Diagnostics;

namespace Dispatcher
{
    class Dispatcher
    {
        List<RefServer> servers = new List<RefServer>();
        private int nextServIndex;
        UDPBroadcaster UDPBroadcasterObj;

        public int NextServIndex
        {
            get
            {
                if (servers.Count == 0) // если серверов нет
                {
                    nextServIndex = -1;
                }
                else if (nextServIndex >= servers.Count) // сбрасываем счетчик round balance
                {
                    nextServIndex = 0;
                }
                else if(servers.Count > 0 && nextServIndex == -1) // если сервера появились, а индекс ещё указывает обратное
                {
                    nextServIndex = 0;
                }

                return nextServIndex++;
            }
            set
            {
                nextServIndex = value;
            }
        }

        internal void UpdateServerList(RefServer refServer)
        {
            servers.Remove(refServer);
            ShowServers();
        }

        string cacheServer = "";
        int priority = 0;

        TCPListener tcpListener;

        public Dispatcher()
        {
            Console.Title = "Dispatcher";

            UDPClient udpClient = new UDPClient();
            
            if (udpClient.IsBroadcasterExists())
            {
                Console.WriteLine("****** Диспетчер уже присутствует в сети. ******");
            }
            else
            {
                //Console.WriteLine("****** Диспетчер запущен ******");

                int tcpport = Convert.ToInt32(ConfigurationManager.AppSettings["dispatcherTCPport"].ToString());
                tcpListener = new TCPListener(tcpport);
                tcpListener.onMessage += handleRequest;
                tcpListener.StartListen();
                Console.WriteLine("TCP слушает по адресу " + tcpListener.Adress);

                UDPBroadcasterObj = new UDPBroadcaster(8555, "239.254.255.255");
                UDPBroadcasterObj.Start();
                ShowServers();
            }
        }

        void handleRequest(IPEndPoint endpoint, MetaData md, string msg)
        {
            MetaData.Roles role = md.Role;

            switch (role)
            {
                case MetaData.Roles.cache:
                    register(endpoint, MetaData.Roles.cache);
                    break;
                case MetaData.Roles.server:
                    register(endpoint, MetaData.Roles.server);
                    break;
                case MetaData.Roles.client:
                    pickServerForClient(endpoint);
                    break;
            }
        }

        void register(IPEndPoint endpoint, MetaData.Roles role)
        {

            if (role == MetaData.Roles.cache)
            {
                cacheServer = endpoint.Address.ToString();
                //Console.WriteLine("Зарегистрирован кэш-сервер по адресу " + endpoint.Address.ToString());

                servers.ToList().ForEach(x => x.SendCacheIP(cacheServer));

            }
            else
            {
                var rs = new RefServer(endpoint, this);
                servers.Add(rs);
                //Console.WriteLine("Зарегистрирован сервер реферирования по адресу " + endpoint.Address.ToString());
                if (cacheServer != String.Empty)
                {
                    rs.SendCacheIP(cacheServer);
                }
            }
            ShowServers();
            string priorityString = priority.ToString();
            MetaData registerResponceMD = new MetaData(MetaData.Roles.dispatcher, MetaData.Actions.register, MetaData.ContentTypes.plainText, priorityString);
            tcpListener.Send(endpoint, priorityString, registerResponceMD);
            priority++; //todo: менять приоритет
        }

        void pickServerForClient(IPEndPoint client)
        {
            if (servers.Count == 0) // если серверов для реферирования нет
            {

                
                MetaData metaData = new MetaData(MetaData.Roles.server, MetaData.Actions.none, MetaData.ContentTypes.error, "0000");
                tcpListener.Send(client, "0000", metaData);
                return;
            }
            string refServIP = servers[NextServIndex].EndPoint.Address.ToString();

            MetaData md = new MetaData(MetaData.Roles.server, MetaData.Actions.none, MetaData.ContentTypes.link, refServIP);

            tcpListener.Send(client, refServIP, md);
        }

        private void ShowServers()
        {
            Console.Clear();
            Console.WriteLine("****** Диспетчер запущен ******");
            Console.WriteLine("подключенные серверы:");
            for (int i = 0; i < servers.Count; i++)
            {
                Console.WriteLine("{0,2}   {1,18}",i, servers[i].EndPoint.ToString());
                Console.WriteLine();   
            }
            Console.WriteLine("Кэш-сервер: {0}", cacheServer);

        }

        ~Dispatcher()
        {
            if(UDPBroadcasterObj != null)
            {
                UDPBroadcasterObj.Stop();
            }

            if (tcpListener != null)
            {
                tcpListener.Close();
            }
            
            Debug.Print("Dispatcher closed");
        }
 
    }
}
