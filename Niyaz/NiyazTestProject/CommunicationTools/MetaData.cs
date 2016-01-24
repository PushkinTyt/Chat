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
        static public int defaultPackageSize = 4096;

        public enum Roles { none, dispatcher, server, cache, client };
        public enum Actions { none, refText, refNews, register, getCacheAdress, ping };
        public enum ContentTypes { link, plainText, xml, binary, none };

        int messageSize = 0;
        Roles role;
        Actions action;
        ContentTypes contentType = ContentTypes.none;

        Encoding encoding = Encoding.Default;

        public MetaData(Roles role, Actions action, ContentTypes contentType)
        {
            this.role = role;
            this.action = action;
            this.contentType = contentType;
        }

        public MetaData(Roles role, Actions action, ContentTypes contentType, string message)
        {
            this.role = role;
            this.action = action;
            this.contentType = contentType;
            messageSize = encoding.GetByteCount(message);
        }

        public MetaData(Roles role, Actions action, ContentTypes contentType, Encoding encoding, string message)
        {
            this.role = role;
            this.action = action;
            this.contentType = contentType;
            this.encoding = encoding;
            messageSize = encoding.GetByteCount(message);
        }

        public MetaData(Roles role, Actions action)
        {
            this.role = role;
            this.action = action;
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
        }

        public int MessageSize
        {
            get
            {
                return messageSize;
            }
        }
    }
}
