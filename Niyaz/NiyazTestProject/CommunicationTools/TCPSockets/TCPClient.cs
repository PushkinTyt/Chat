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
        Socket client;
        //bool shouldWork = true;

        Thread messageHandler;

        //Описываем сигнатуру метода-обработчика
        public delegate void ReceiveMethod(MetaData md, string msg);
        //Событие, на которое будут подписываться
        public event ReceiveMethod onMessage;

        public TCPClient(string ip, int port)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), port);

            client = new Socket(SocketType.Stream, ProtocolType.Tcp);

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

        public bool Send(string msg, MetaData md)
        {
            byte[] buffer;

            //Сериализуем и шлем пакет метаданных
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, md);
            ms.Position = 0;
            buffer = new byte[ms.Length];
            ms.Read(buffer, 0, Convert.ToInt32(ms.Length));
            try
            {
                client.Send(buffer);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return false;
            }


            MemoryStream msgStream = new MemoryStream();
            byte[] bytes = md.Encoding.GetBytes(msg);
            msgStream.Write(md.Encoding.GetBytes(msg), 0, bytes.Length);

            //Шлем само сообщение
            buffer = new byte[md.MessageSize];
            msgStream.Position = 0;
            msgStream.Read(buffer, 0, md.MessageSize);
            try
            {
                client.Send(buffer);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Отправка метаданных без последующих данных (для отправки команд)
        /// </summary>
        /// <param name="md"></param>
        /// <returns></returns>
        public bool Send(MetaData md)
        {
            byte[] buffer;

            //Сериализуем и шлем пакет метаданных
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, md);
            ms.Position = 0;
            buffer = new byte[ms.Length];
            ms.Read(buffer, 0, Convert.ToInt32(ms.Length));
            try
            {
                client.Send(buffer);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Принимает сообщения от сервера и генерирует соответсвующее событие
        /// </summary>
        public void ReceiveData()
        {
            int bufferSize = MetaData.defaultPacketSize;
            while (true)
            {
                string msg = "";

                byte[] buffer = new byte[bufferSize];

                try
                {
                    client.Receive(buffer);
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

                if(md.MessageSize > 0)
                {
                    buffer = new byte[md.MessageSize];
                    try
                    {
                        client.Receive(buffer);
                    }
                    catch (SocketException ex)
                    {
                        Debug.Print(ex.Message);
                        break;
                    }

                    msg += md.Encoding.GetString(buffer);

                    msg = msg.TrimEnd('\0'); //В UTF-8 в конце строки куча \0, консоли это не нравится
                }

                onMessage(md, msg);
            }
        }

        /// <summary>
        /// Разрывает подключение с сервером
        /// </summary>
        public void Close()
        {
            client.Close();
            messageHandler.Abort();
        }

        ~TCPClient()
        {
            this.Close();
        }
    }
}
