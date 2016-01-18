using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using System.Threading;

namespace CommunicationTools
{
    class MSMQListener
    {
        Thread subscribeHandler;
        MessageQueue mq;
        bool shouldWork;

        //Описываем сигнатуру метода-обработчика 
        public delegate void ReceiveMethod(string msg);
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
            TimeSpan timeout = new TimeSpan(0, 0, 5); //Ждем сообщение максимум 5 секунд (чтобы не застрять в потоке навеки)

            while (shouldWork)
            {
                //Ждем пока не появится сообщение
                Message msg = mq.Peek(timeout);

                //Отправляяем подписчикам
                if (msg != null && onMessage != null)
                {
                    mq.Receive();
                    onMessage(msg.Body.ToString());
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
            shouldWork = false;
            if (subscribeHandler != null)
            {
                subscribeHandler.Abort();
            }
        }
    }
}
