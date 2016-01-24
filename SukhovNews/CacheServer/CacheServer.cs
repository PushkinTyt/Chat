using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationTools;
using CommunicationTools.ComComponent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Configuration;
using System.ServiceModel.Description;
using System.Threading;

namespace CacheServer
{
    class CacheServer
    {
        DispatcherClient dc;
        ServiceHost host;

        //URL, Физический путь к файлу
        MSMQListener msmqListener;

        private static ManualResetEvent _ResetEvent = new ManualResetEvent(false);

        public CacheServer()
        {
            dc = new DispatcherClient(MetaData.Roles.cache);
            dc.onFound += dispFound;
            dc.StartListen();

            if (!Directory.Exists(CachedFile.CacheFolder))
            {
                Directory.CreateDirectory(CachedFile.CacheFolder);
            }

            string msmqName = ConfigurationManager.AppSettings["cacheMSMQName"];
            msmqListener = new MSMQListener(msmqName);
            msmqListener.onMessage += MsmqListener_onMessage;
            msmqListener.StartListen();
        }

        private void MsmqListener_onMessage(string msg, string tag)
        {
            CachedFile file;
            try
            {
                file = CacheService.files[tag];
            }
            catch
            {
                file = null;
            }
            

            if (file != null)
            {
                file.WriteFile(tag, msg);
            }
            else
            {
                file = new CachedFile();
                file.WriteFile(tag, msg);
                CacheService.files.Add(tag, file);
            }
        }

        //Действия, при нахождении диспетчера
        void dispFound()
        {
            dc.Register();
        }
    }
}
