using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using CommunicationTools;
using System.Configuration;

namespace Dispatcher
{
    class RefServer : IEquatable<RefServer>
    {
        IPEndPoint endPoint;
        TCPClient client;

        public RefServer(IPEndPoint endPoint)
        {
            endPoint.Port = Convert.ToInt32(ConfigurationManager.AppSettings["refServerPort"].ToString());
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
            MetaData md = new MetaData(MetaData.Roles.dispatcher, MetaData.Actions.getCacheAdress, MetaData.ContentTypes.plainText, IP);
            if (!client.Send(IP, md))
                Console.WriteLine("Suka");
        }

        bool IEquatable<RefServer>.Equals(RefServer other)
        {
            return other.EndPoint.Address.Equals(this.EndPoint.Address);
        }
    }
}
