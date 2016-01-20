using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Runtime.Serialization;


namespace CommunicationTools
{
    [Serializable]
    public class MetaData
    {
        //Дефолтные параметры используются при пересылке метаданных (т.е. этого класса)
        [NonSerialized]
        static public int defaultPacketSize = 2048;

        public enum Roles { Dispatcher, Server, Cache, Client };
        public enum Actions { none, refText, refNews };

        uint packageCount;
        int bufferSize = 1024;
        Roles role;
        Actions action;
        string body;

        Encoding encoding = Encoding.Default;

        public MetaData(int bufferSize, Roles role, Actions action)
        {
            this.bufferSize = bufferSize;
            this.role = role;
            this.action = action;
        }

        public MetaData(Roles role, Actions action)
        {
            this.role = role;
            this.action = action;
        }

        /// <summary>
        /// Размер пакета
        /// </summary>
        public int BufferSize
        {
            get
            {
                return bufferSize;
            }

            set
            {
                bufferSize = value;
            }
        }

        public uint PackageNum
        {
            get
            {
                return packageCount;
            }
        }

        /// <summary>
        /// Дополнительная служебная информация
        /// </summary>
        public string Body
        {
            get
            {
                return body;
            }

            set
            {
                body = value;
            }
        }

        /// <summary>
        /// Роль пересылающего
        /// </summary>
        public Roles Role
        {
            get
            {
                return role;
            }

            set
            {
                role = value;
            }
        }

        /// <summary>
        /// Требуемое действия от получателя
        /// </summary>
        public Actions Action
        {
            get
            {
                return action;
            }

            set
            {
                action = value;
            }
        }

        /// <summary>
        /// Кодировка сообщения
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                return encoding;
            }

            set
            {
                encoding = value;
            }
        }

        /// <summary>
        /// Подсчет количества необходимых пакетов для пересылки сообщения
        /// </summary>
        /// <param name="msg"></param>
        public void CalcMsgData(string msg)
        {
            //деление с округлением в большую сторону
            double fPackageNum = Math.Ceiling((double)Encoding.GetByteCount(msg) / bufferSize);
            packageCount = Convert.ToUInt32(fPackageNum);
        }
    }
}
