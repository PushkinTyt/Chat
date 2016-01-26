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
                if(broadcastListener != null)
                {
                    broadcastListener.Abort();
                }
                broadcastListener = new Thread(new ThreadStart(BroadcastReciveProcess));
                broadcastListener.Start();
            }
            catch (Exception ex)
            {
                onError(ex.Message);
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
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, remotePort);

                //Волшебные строки для запуска нескольких приложений, использующих один адрес или порт
                udpReciver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpReciver.ExclusiveAddressUse = false;
                udpReciver.Client.Bind(ip);

                udpReciver.Client.ReceiveTimeout = 5000;

                while (true)
                {

                    //IPEndPoint ip = new IPEndPoint(IPAddress.Any, remotePort);
                    byte[] data;
                    try
                    {
                        data = udpReciver.Receive(ref ip);

                        string message = Encoding.Unicode.GetString(data);
                        dispatcherEndPoint = ip;
                        if (onMessage != null)
                        {
                            onMessage(dispatcherEndPoint, message);
                        }
                    }
                    catch(SocketException ex)
                    { }

                    Thread.Sleep(300);
                }
            }

            catch (ThreadAbortException)
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
