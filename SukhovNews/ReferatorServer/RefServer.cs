using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using CommunicationTools.ComComponent;
using CommunicationTools;
using System.Configuration;
using System.Net;
using Rss;
using Referat;


namespace ReferatorServer
{
    public class RefServer
    {
        DispatcherClient dc;
        CacheServer cs;
        MSMQClient cacheMSMQ;

        TCPListener tcpListener;

        public RefServer()
        {
            int tcpPort = Convert.ToInt32(ConfigurationManager.AppSettings["refServerPort"].ToString());
            tcpListener = new TCPListener(tcpPort);
            tcpListener.onMessage += TcpListener_onMessage;
            tcpListener.StartListen();

            dc = new DispatcherClient(MetaData.Roles.server);
            dc.onFound += dispFound;
            dc.StartListen();
        }

        private void TcpListener_onMessage(IPEndPoint endpoint, MetaData md, string msg)
        {
            MetaData.Roles role = md.Role;

            switch(role)
            {
                case MetaData.Roles.dispatcher:
                    if(md.Action == MetaData.Actions.getCacheAdress)
                    {
                        conToCacheServer(msg);
                    }
                    break;

                case MetaData.Roles.client:
                    if(md.Action == MetaData.Actions.refNews)
                    {
                        Referator referator = null;
                        string fullArticle = "";
                        string[] refParameters = msg.Split('|');
                        string url = refParameters[0];
                        byte compressPerc = byte.Parse(refParameters[1]);
                        // hack: здесь нужно дописать запрос на кэшированную версию
                        // пока что всегда реферируем без проверки на кэш
                        bool hasCache = false;
                        if (hasCache) // если есть кэш
                        {
                            //Referator referator = new Referator(cachedXML); //создаем рефератор из кэшированного xml
                        }
                        else
                        {
                            try
                            {
                                // берем статью из интернета
                                HtmlParser hp = new HtmlParser(url);
                                fullArticle = hp.Text;
                                referator = new Referator(fullArticle, "utf-8");
                            }
                            catch (Exception ex)
                            {
                                //todo: обработать ошибку если не удалось скачать статью
                            }
                        }

                        string compressedArticle = referator.Compress(compressPerc);

                        MetaData articleMD = new MetaData(MetaData.Roles.server, MetaData.Actions.refNews, MetaData.ContentTypes.plainText, fullArticle);
                        tcpListener.Send(endpoint, fullArticle, articleMD);
                    }
                    break;
            }
        }

        void conToCacheServer(string ip)
        {
            UriBuilder uriBuilder = new UriBuilder(ip);
            uriBuilder.Port = Convert.ToInt32(ConfigurationManager.AppSettings["cacheServicePort"].ToString());

            cs = new CacheServer(uriBuilder.Uri.ToString());

            Console.WriteLine("Запущен клиент сервиса кэш-сервера");

            string msmqName = ConfigurationManager.AppSettings["cacheMSMQName"];
            cacheMSMQ = new MSMQClient(ip, msmqName);
        }

        //Действия, при нахождении диспетчера
        void dispFound()
        {
            Console.WriteLine("Попытка зарегистрироваться на диспетчере");
            dc.Register();
        }

        /// <summary>
        /// реферирование статьи по url
        /// </summary>
        /// <param name="Text">полный текст статьи</param>
        /// <returns>строка xml</returns>
        string refNews(string Text)
        {
            //bool cacheExists;
            //bool passed;

            //string rangeSentences;

            //cs.cacheFileExists(URL, out cacheExists, out passed);
            //if(cacheExists && passed)
            //{
            //    rangeSentences = cs.getCachedFile(URL); 
            //}
            //else
            //{
            //    cs.notifyReferation(URL);
            //    //Реферирование
            //    if(cacheMSMQ != null)
            //    {
            //        cacheMSMQ.Send("XMLTEXT", URL); //Вместо первого аргумента сериализованный реферат
            //    }
            //}



            return "hey! You are connected";
        }
    }
}
