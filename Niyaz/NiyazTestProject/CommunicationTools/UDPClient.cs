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
                if(broadcastListener != null)
                {
                    broadcastListener.Abort();
                }
                broadcastListener = new Thread(new ThreadStart(BroadcastReciveProcess));
                broadcastListener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Stop()
        {
            broadcastListener.Abort();
        }

        private void BroadcastReciveProcess()
        {
            UdpClient udpReciver = new UdpClient();
            try
            {
                
                udpReciver.ExclusiveAddressUse = false;
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, 8555);

                //Волшебные строки для запуска нескольких приложений, использующих один адрес или порт
                udpReciver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpReciver.ExclusiveAddressUse = false;
                udpReciver.Client.Bind(ip);

                while (true)
                {
                    byte[] data = udpReciver.Receive(ref ip);
                    string message = Encoding.Unicode.GetString(data);
                    dispatcherEndPoint = ip;
                    if (onMessage != null)
                    {
                        onMessage(dispatcherEndPoint, message);
                    }
                    Thread.Sleep(300);
                }
            }
            catch(ThreadAbortException)
            {
                udpReciver.Close();
                return;
            }

        }

        ~UDPClient()
        {
            broadcastListener.Abort();
        }
    }
}
