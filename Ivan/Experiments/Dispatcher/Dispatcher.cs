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
        HashSet<RefServer> servers = new HashSet<RefServer>();
        string cacheServer = "";

        public Dispatcher()
        {
            Console.Title = "Dispatcher";
            Console.WriteLine("****** Диспетчер запущен ******");

            int tcpport = Convert.ToInt32(ConfigurationManager.AppSettings["dispatcherTCPport"].ToString());
            TCPListener listener = new TCPListener(tcpport);
            listener.onMessage += handleRequest;
            listener.StartListen();
            Console.WriteLine("TCP слушает по адресу " + listener.Adress);

            UDPBroadcaster UDPBroadcasterObj = new UDPBroadcaster(8555, "239.254.255.255");
            UDPBroadcasterObj.Start();
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
        }

        void pickServerForClient(IPEndPoint client)
        {
            //Балансировка
        }


    }
}
