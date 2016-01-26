﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Threading;
using System.Diagnostics;

namespace CommunicationTools.ComComponent
{
    public class DispatcherClient
    {
        IPEndPoint dispEndPoint = null;

        int priority = 0;
        int waitCoef = 600;

        MetaData.Roles role;
        TCPClient tcpClient;
        UDPClient udpClient = new UDPClient();

        bool connected = false;

        public DispatcherClient(MetaData.Roles role)
        {
            this.role = role;
        }
        
        public void StartListen()
        {
            if(udpClient != null)
                udpClient.onMessage += receiveBroadcastMessage;
            udpClient.Start();
            Thread.Sleep(UDPClient.BroadcastInterval + 500); //Слушаем broadcaster'a на 500 миллисекунд больше
            if(!connected)
            {
                startDispatcher();
            }
        }

        void receiveBroadcastMessage(IPEndPoint endPoint, string message)
        {
            dispEndPoint = endPoint;
            Console.WriteLine(String.Format("Найден диспетчер по адресу {0}. Сообщение от диспетчера: {1}", endPoint.Address.ToString(), message));

            try
            {
                int port = Convert.ToInt32(ConfigurationManager.AppSettings["dispatcherTCPport"].ToString());
                tcpClient = new TCPClient(endPoint.Address.ToString(), port);
                tcpClient.onMessage += TcpClient_onMessage;
                tcpClient.onDisconnect += TcpClient_onDisconnect;
                tcpClient.StartListen();

                onFound();
                udpClient.Stop();
                connected = true;
            }
            catch(SocketException ex)
            {
                Console.WriteLine("Не удалось присоединиться к диспетчеру по адресу " + endPoint.Address.ToString() + "ошибка: " + ex.Message);
            }
        }

        private void TcpClient_onMessage(MetaData md, string msg)
        {
            switch(md.Action)
            {
                case MetaData.Actions.register:
                    priority = Int32.Parse(msg);
                    Console.WriteLine("Получен порядковый номер " + msg);
                    break;
            }
        }

        private void TcpClient_onDisconnect()
        {
            connected = false;

            Console.WriteLine("Прервано соединение с диспетчером...");
            startDispatcher();
        }

        void startDispatcher()
        {
            udpClient.Start();
            Thread.Sleep(priority * waitCoef + UDPClient.BroadcastInterval);
            if (!connected)
            {
                string dispPath = ConfigurationManager.AppSettings["dispatcherPath"].ToString();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = dispPath;
                Process dispProcess = Process.Start(startInfo);
                Console.WriteLine("Запущен диспетчер на этой машине.");
            }
        }

        public delegate void dispFound();
        public event dispFound onFound;

        public void Register()
        {
            MetaData md = new MetaData(role, MetaData.Actions.register);
            try
            {
                tcpClient.Send("", md);
                Console.WriteLine("Регистрация на сервера пройдена");
                connected = true;
            }
            catch(SocketException ex)
            {
                Console.WriteLine("Не удалось зарегестрироваться на сервере. " + ex.Message);
            }
        }
    }
}
