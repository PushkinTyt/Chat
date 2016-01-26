using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ReferatorServer
{
    class Program
    {
        private static ManualResetEvent _ResetEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            RefServer server = new RefServer();
            //server.Register();
            _ResetEvent.WaitOne();
        }
    }
}
