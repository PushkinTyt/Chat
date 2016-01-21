using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using CommunicationTools.ComComponent;
using CommunicationTools;

namespace ReferatorServer
{
    public class RefServer
    {
        DispatcherClient dc;

        public RefServer()
        {
            dc = new DispatcherClient(MetaData.Roles.server);
            dc.connectWithDisp();
        }
    }
}
