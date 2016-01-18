using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;

namespace CommunicationTools
{
    class MSMQClient
    {
        MessageQueue mq;

        public MSMQClient(string remoteMSMQip, string MSMQname)
        {
            mq = new MessageQueue("FormatName:DIRECT=TCP:" + remoteMSMQip + @"\private$\" + MSMQname);
            mq.Formatter = new XmlMessageFormatter(new Type[] { typeof(String) });
        }

        public void Send(string msg)
        {
            mq.Send(msg);
        }

    }
}
