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

        public Dispatcher()
        {
            Console.Title = "Dispatcher";
            Console.WriteLine("****** Диспетчер запущен ******");

            int tcpport = Convert.ToInt32(ConfigurationManager.AppSettings["dispatcherTCPport"].ToString());
            TCPSListener listener = new TCPSListener(tcpport);
            listener.onMessage += handleRequest;
            listener.StartListen();
            Console.WriteLine("TCP слушает по адресу " + listener.Adress);

            UDPBroadcaster UDPBroadcasterObj = new UDPBroadcaster(8555, "239.254.255.255");
            UDPBroadcasterObj.Start();
        }

        void handleRequest(IPEndPoint endpoint, MetaData md, string msg)
        {
            MetaData.Roles role = md.Role;

            //Херня. При добавлении новых функций - придется переписывать эту часть.
            //TODO: придумать что-то поадекватнее
            switch (role)
            {
                case MetaData.Roles.cache:
                case MetaData.Roles.server:
                    register(endpoint, role);
                    break;
                case MetaData.Roles.client:
                    pickServerForClient(endpoint);
                    break;
            }
        }

        void register(IPEndPoint endpoint, MetaData.Roles role)
        {
            if(role == MetaData.Roles.cache)
            {
                //Добавление кэш-сервера
            }
            else
            {
                servers.Add(new RefServer(endpoint));
                Console.WriteLine("Зарегистрирован сервер реферирования по адресу " + endpoint.Address.ToString());
            }
        }

        void pickServerForClient(IPEndPoint client)
        {
            //Балансировка
        }


    }
}
