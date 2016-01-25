using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CommunicationTools
{
    public class UDPClient
    {
        const int remotePort = 8555;
        private static IPAddress remoteAddress;
        private static IPEndPoint dispatcherEndPoint = null;
        Thread broadcastListener;

        public delegate void receiveBroadcastMessage(IPEndPoint endPoint, string message);
        public event receiveBroadcastMessage onMessage;

        public delegate void Error(string errorDescr);
        public event Error onError;

        public void Start()
        {
            //remoteAddress = IPAddress.Parse(remoteIP);
            try
            {
                if(broadcastListener == null)
                    broadcastListener = new Thread(new ThreadStart(BroadcastReciveProcess));
                broadcastListener.Start();
            }
            catch (Exception ex)
            {
                onError(ex.Message);
            }
        }

        public void Pause()
        {
            broadcastListener.Suspend();
        }


        private void BroadcastReciveProcess()
        {
            UdpClient udpReciver = new UdpClient(remotePort);
            try
            {
                while (true)
                {
                    IPEndPoint ip = new IPEndPoint(IPAddress.Any, remotePort);
                    byte[] data = udpReciver.Receive(ref ip);
                    string message = Encoding.Unicode.GetString(data);
                    dispatcherEndPoint = ip;
                    if (onMessage != null)
                    {
                        onMessage(dispatcherEndPoint, message);
                    }
                }
            }
            catch (Exception ex)
            {
                onError(ex.Message);
            }
            finally
            {
                udpReciver.Close();
            }
        }
    }
}
