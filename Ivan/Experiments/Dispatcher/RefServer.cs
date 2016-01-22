using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using CommunicationTools;

namespace Dispatcher
{
    class RefServer
    {
        IPEndPoint endPoint;
        TCPClient client;

        public RefServer(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;

            client = new TCPClient(endPoint.Address.ToString(), endPoint.Port);
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
            MetaData md = new MetaData(MetaData.Roles.dispatcher, MetaData.Actions.getCacheAdress);
            client.Send(IP, md);
        }
    }
}
