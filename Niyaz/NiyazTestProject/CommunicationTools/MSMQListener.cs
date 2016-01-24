using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using System.Threading;

namespace CommunicationTools
{
    public class MSMQListener
    {
        Thread subscribeHandler;
        MessageQueue mq;

        //Описываем сигнатуру метода-обработчика 
        public delegate void ReceiveMethod(string msg, string label);
        //Событие, на которое будут подписываться
        public event ReceiveMethod onMessage;

        /// <summary>
        /// Инициирует экземпляр класса для ПОЛУЧЕНИЯ сообщений посредством MSMQ
        /// </summary>
        /// <param name="localMSMQname">Имя очереди сообщений</param>
        public MSMQListener(string localMSMQname)
        {
            string msmqName = @".\private$\" + localMSMQname;

            if (!MessageQueue.Exists(msmqName))
            {
                mq = MessageQueue.Create(msmqName);
            }
            else
            {
                mq = new MessageQueue(msmqName);
            }

            mq.Formatter = new XmlMessageFormatter(new Type[] { typeof(String) });
        }

        /// <summary>
        /// Метод, запускающий поток для обработки клиентов, подписавшихся на событие onMessage
        /// </summary>
        public void StartListen()
        {
            subscribeHandler = new Thread(sendToSubscriber);
            subscribeHandler.Start();
        }

        void sendToSubscriber()
        {
            while (true)
            {
                //Ждем пока не появится сообщение
                try
                {
                    Message msg = mq.Peek();

                    //Отправляяем подписчикам
                    if (msg != null && onMessage != null)
                    {
                        mq.Receive();
                        onMessage(msg.Body.ToString(), msg.Label);
                    }
                }
                catch
                {
                    mq.Close();
                    break;
                }

            }
        }

        //Синхронное получение
        public string ReceiveMessage()
        {
            return mq.Receive().Body.ToString();
        }

        public void Close()
        {
            if (subscribeHandler != null)
            {
                subscribeHandler.Abort();
            }
        }
    }
}
