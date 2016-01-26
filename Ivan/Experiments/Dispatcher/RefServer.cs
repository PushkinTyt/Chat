using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using CommunicationTools;
using System.Configuration;
using System.Net.Sockets;
using System.Diagnostics;

namespace Dispatcher
{
    class RefServer : IEquatable<RefServer>
    {
        IPEndPoint endPoint;
        TCPClient client;
        Dispatcher parentDispatcher;
        public RefServer(IPEndPoint endPoint, Dispatcher dispatcher)
        {
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["refServerPort"].ToString());
            this.endPoint = new IPEndPoint(endPoint.Address, port);
            parentDispatcher = dispatcher;
            client = new TCPClient(this.endPoint.Address.ToString(), this.endPoint.Port);
            client.onDisconnect += Client_onDisconnect;
        }

        private void Client_onDisconnect()
        {
            parentDispatcher.UpdateServerList(this);
            client.Close();
        }

        public IPEndPoint EndPoint
        {
            get
            {
                return endPoint;
            }
        }

        public void SendCacheIP(string IP)
        {
            MetaData md = new MetaData(MetaData.Roles.dispatcher, MetaData.Actions.getCacheAdress, MetaData.ContentTypes.plainText, IP);
            try
            {
                client.Send(IP, md);
            }
            catch(SocketException ex)
            {
                Console.WriteLine("Не удалось отправить IP кэш-сервера серверу реферирования по адресу " + IP);
            }
        }

        bool IEquatable<RefServer>.Equals(RefServer other)
        {
            return other.EndPoint.Address.Equals(this.EndPoint.Address);
        }

        //~RefServer()
        //{
        //    client.Close();
        //    Debug.Print("RefServer closed");
        //}
    }
}
