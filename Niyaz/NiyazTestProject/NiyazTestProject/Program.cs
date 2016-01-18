using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationTools;
using System.Net;

namespace NiyazTestProject
{
    class Program
    {
        static TCPSListener listener;
        static TCPClient client;
        static TCPClient client1;

        static void Main(string[] args)
        {
            //Console.OutputEncoding = Encoding.U;

            //Объвляем сокет-сервер (к нему будут подключаться клиенты)
            listener = new TCPSListener("127.0.0.1", 1452);
            listener.onMessage += ReceiveMessage;
            listener.StartListen();

            Console.WriteLine("Запущен сокет-сервер по адресу " + listener.Adress);
            

            client = new TCPClient("127.0.0.1", 1452);
            client.onMessage += ReceiveMessage;
            client.StartListen();

            client1 = new TCPClient("127.0.0.1", 1452);
            client1.onMessage += ReceiveMessage;
            client1.StartListen();

            while (true)
            {
                Console.ReadKey();
                client.Send("13456");
                client1.Send("Второй клиент");
            }
        }

        static void ReceiveMessage(IPEndPoint endpoint, string msg)
        {
            Console.WriteLine(String.Format("{0}: {1}", endpoint.Address, msg));
            listener.SendToClient(endpoint, "Hello!");
        }

        static void ReceiveMessage(string msg)
        {
            Console.WriteLine(String.Format("{0}: {1}", "server", msg));
        }
    }
}
