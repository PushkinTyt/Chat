using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationTools;
using System.Configuration;
using System.Net;

namespace Dispatcher
{
    class Dispatcher
    {
        List<RefServer> servers = new List<RefServer>();
        private int nextServIndex;

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
                Console.WriteLine("****** Диспетчер запущен ******");

                int tcpport = Convert.ToInt32(ConfigurationManager.AppSettings["dispatcherTCPport"].ToString());
                tcpListener = new TCPListener(tcpport);
                tcpListener.onMessage += handleRequest;
                tcpListener.StartListen();
                Console.WriteLine("TCP слушает по адресу " + tcpListener.Adress);

                UDPBroadcaster UDPBroadcasterObj = new UDPBroadcaster(8555, "239.254.255.255");
                UDPBroadcasterObj.Start();
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
                Console.WriteLine("Зарегистрирован кэш-сервер по адресу " + endpoint.Address.ToString());

                servers.ToList().ForEach(x => x.SendCacheIP(cacheServer));
            }
            else
            {
                var rs = new RefServer(endpoint);
                servers.Add(rs);
                Console.WriteLine("Зарегистрирован сервер реферирования по адресу " + endpoint.Address.ToString());
                if (cacheServer != String.Empty)
                {
                    rs.SendCacheIP(cacheServer);
                }
            }
       
            string priorityString = priority.ToString();
            MetaData registerResponceMD = new MetaData(MetaData.Roles.dispatcher, MetaData.Actions.register, MetaData.ContentTypes.plainText, priorityString);
            tcpListener.Send(endpoint, priorityString, registerResponceMD);
            priority++;
        }

        void pickServerForClient(IPEndPoint client)
        {
            if (NextServIndex == -1)
            {
                // todo: написать как будет себя вести диспетчер, если после запроса клиента на реферирование оказалось, что серверов нет
                throw new NotImplementedException();
            }
            string refServIP = servers[NextServIndex].EndPoint.Address.ToString();

            MetaData md = new MetaData(MetaData.Roles.server, MetaData.Actions.none, MetaData.ContentTypes.link, refServIP);

            tcpListener.Send(client, refServIP, md);
        }
 
    }
}
