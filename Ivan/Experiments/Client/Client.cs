using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Client
{
    class Client
    {
        
        private static string remoteIP = "239.254.255.255";
        const int remotePort = 8555;
        private static IPAddress remoteAddress;
        private static IPEndPoint dispatcherEndPoint = null;

        static void Main(string[] args)
        {
            Console.Title = "Client";
            Console.WriteLine("клиент запущен");

            remoteAddress = IPAddress.Parse(remoteIP);
            try
            {
                Thread broadcastListener = new Thread(new ThreadStart(BroadcastReciveProcess));
                broadcastListener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }

        private static void BroadcastReciveProcess()
        {
            UdpClient udpReciver = new UdpClient(remotePort);
            
            //udpReciver.JoinMulticastGroup(remoteAddress, 10);
            try
            {
                while (true)
                {
                    IPEndPoint ip = new IPEndPoint(IPAddress.Any, 1700);
                    byte[] data = udpReciver.Receive(ref ip);
                    string message = Encoding.Unicode.GetString(data);
                    Console.WriteLine(message + "   from: " + ip.ToString());
                    dispatcherEndPoint = ip;
                }
                /*
                TcpClient tcpClient = new TcpClient(dispatcherEndPoint.Address.ToString(), 8777);
                NetworkStream ns = tcpClient.GetStream();
                BinaryWriter writer = new BinaryWriter(ns);
                byte[] msg = Encoding.Unicode.GetBytes("hello");
                
                writer.Write(msg);

                
                
                
                Thread.Sleep(3000);

                byte[] msg2 = Encoding.Unicode.GetBytes("hello");
                tcpClient.Close();
                writer.Close();
                ns.Close();
                */
                /*
                while (true)
                {

                }
                */
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
