

using System;

namespace Comunicate
{
    public class SocketConnect : IComunicate
    {
        private string address;
        private string port;

        public SocketConnect(string address, string port)
        {

        }

        public object Recive()
        {
            throw new NotImplementedException();
        }

        public void Send(object message)
        {
            throw new NotImplementedException();
        }
    }


}
