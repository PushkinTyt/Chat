using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Dispatcher
{
    class RefServer
    {
        IPEndPoint endPoint;

        public RefServer(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
        }

        public IPEndPoint EndPoint
        {
            get
            {
                return endPoint;
            }
        }
    }
}
