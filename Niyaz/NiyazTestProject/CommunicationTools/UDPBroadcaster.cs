﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CommunicationTools
{
    /// <summary>
    /// Класс для широковещательной рассылки
    /// </summary>
    public class UDPBroadcaster
    {
        private static int broadcastInterval = 5000; // 5сек
        private static string remoteIP; // "239.254.255.255";
        private int remotePort; //8555;
        private static IPAddress remoteAddress;
        const string broadcastMessage = "Dispatcher";
        Thread broadcastThread;


        public UDPBroadcaster(int Port, string remoteIPAdr)
        {
            remotePort = Port;
            remoteIP = remoteIPAdr;
        }

        public void Start()
        {
            if(broadcastThread == null)
                broadcastThread = new Thread(BroadcastSendProcess);
            broadcastThread.Start();
        }

        public void Pause()
        {
            broadcastThread.Suspend();
        }


        private void BroadcastSendProcess()
        {
            UdpClient broadcastSender = new UdpClient();
            remoteAddress = IPAddress.Parse(remoteIP);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, remotePort);
            try
            {
                while (true)
                {
                    DateTime dt = DateTime.Now;
                    string message = "disp";
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    broadcastSender.Send(data, data.Length, endPoint); // отправка
                    Thread.Sleep(broadcastInterval);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                broadcastSender.Close();
            }
        }
    }
}