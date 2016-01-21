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
        private static string remoteIP = "239.254.255.255";
        const int remotePort = 8555;
        private static IPAddress remoteAddress;
        private static IPEndPoint dispatcherEndPoint = null;
        Thread broadcastListener;

        public delegate void receiveBroadcastMessage(IPEndPoint endPoint, string message);
        public event receiveBroadcastMessage onMessage;

        public void Start()
        {
            remoteAddress = IPAddress.Parse(remoteIP);
            try
            {
                if(broadcastListener == null)
                    broadcastListener = new Thread(new ThreadStart(BroadcastReciveProcess));
                broadcastListener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                    IPEndPoint ip = new IPEndPoint(IPAddress.Any, 8555);
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
                Console.WriteLine(ex.Message);
            }
            finally
            {
                udpReciver.Close();

            }
        }
    }
}
