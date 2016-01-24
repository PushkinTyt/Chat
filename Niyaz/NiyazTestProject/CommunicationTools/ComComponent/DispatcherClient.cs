﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Configuration;

namespace CommunicationTools.ComComponent
{
    public class DispatcherClient
    {
        IPEndPoint dispEndPoint = null;

        MetaData.Roles role;
        TCPClient tcpClient;
        UDPClient udpClient = new UDPClient();

        public DispatcherClient(MetaData.Roles role)
        {
            this.role = role;
        }
        
        public void StartListen()
        {
            if(udpClient != null)
                udpClient.onMessage += receiveBroadcastMessage;
            udpClient.Start();
        }

        void receiveBroadcastMessage(IPEndPoint endPoint, string message)
        {
            dispEndPoint = endPoint;
            Console.WriteLine(String.Format("Найден диспетчер по адресу {0}. Сообщение от диспетчера: {1}", endPoint.Address.ToString(), message));

            int port = Convert.ToInt32(ConfigurationManager.AppSettings["dispatcherTCPport"].ToString());
            tcpClient = new TCPClient(endPoint.Address.ToString(), port);

            onFound();
            udpClient.Pause();
        }

        public delegate void dispFound();
        public event dispFound onFound;

        public void Register()
        {
            MetaData md = new MetaData(role, MetaData.Actions.register);
            if(tcpClient.Send("", md))
            {
                Console.WriteLine("Регистрация на сервера пройдена");
            }
        }
    }
}
