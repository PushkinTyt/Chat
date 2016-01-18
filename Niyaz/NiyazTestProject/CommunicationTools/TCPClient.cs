using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace CommunicationTools
{
    public class TCPClient
    {
        TcpClient client;
        bool shouldWork = true;

        Thread messageHandler;

        //Описываем сигнатуру метода-обработчика 
        public delegate void ReceiveMethod(string msg);
        //Событие, на которое будут подписываться
        public event ReceiveMethod onMessage;

        public TCPClient(string ip, int port)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), port);

            client = new TcpClient();

            try
            {
                client.Connect(endpoint);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Начинает прослушивание сообщений, пришедших от сервера
        /// </summary>
        public void StartListen()
        {
            messageHandler = new Thread(ReceiveData);
            messageHandler.Start();
        }

        /// <summary>
        /// Отправляет сообщение сокету-серверу
        /// </summary>
        /// <param name="msg"></param>
        public void Send(string msg)
        {
            if (client.Connected)
            {
                byte[] buffer;

                MetaData md = new MetaData(MetaData.Roles.Client, MetaData.Actions.none);
                //md.Encoding = Encoding.Unicode;
                md.CalcMsgData(msg);

                //Сериализуем и шлем пакет метаданных
                MemoryStream ms = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, md);
                ms.Position = 0;
                buffer = new byte[ms.Length];
                ms.Read(buffer, 0, Convert.ToInt32(ms.Length));
                client.Client.Send(buffer);

                MemoryStream msgStream = new MemoryStream();
                byte[] bytes = md.Encoding.GetBytes(msg);
                msgStream.Write(md.Encoding.GetBytes(msg), 0, bytes.Length);

                //Шлем само сообщение
                buffer = new byte[md.BufferSize];
                msgStream.Position = 0;
                int readCount = 0;
                while(readCount < msgStream.Length)
                {
                    readCount += msgStream.Read(buffer, 0, md.BufferSize);
                    client.Client.Send(buffer);
                }
            }
            else
            {
                throw new Exception("Подключение не удалось");
            }
        }

        /// <summary>
        /// Принимает сообщения от сервера и генерирует соответсвующее событие
        /// </summary>
        public void ReceiveData()
        {
            int bufferSize = MetaData.defaultPacketSize;
            while (shouldWork)
            {
                string msg = "";

                byte[] buffer = new byte[bufferSize];

                try
                {
                    client.Client.Receive(buffer);
                }
                catch (SocketException ex)
                {
                    Debug.Print(ex.Message);
                    break;
                }
                MemoryStream ms = new MemoryStream();

                ms.Write(buffer, 0, bufferSize);
                ms.Position = 0;
                BinaryFormatter formatter = new BinaryFormatter();
                MetaData md = (MetaData)formatter.Deserialize(ms);

                buffer = new byte[md.BufferSize];
                for (int i = 0; i < md.PackageNum; i++)
                {
                    try
                    {
                        client.Client.Receive(buffer);
                    }
                    catch (SocketException ex)
                    {
                        Debug.Print(ex.Message);
                        break;
                    }
                    msg += md.Encoding.GetString(buffer);
                }


                msg = msg.TrimEnd('\0'); //В UTF-8 в конце строки куча \0, консоли это не нравится
                onMessage(msg);
            }
        }

        /// <summary>
        /// Разрывает подключение с сервером
        /// </summary>
        public void Close()
        {
            shouldWork = false;
            messageHandler.Abort();
        }
    }
}
