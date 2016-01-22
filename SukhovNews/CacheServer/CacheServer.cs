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

namespace CacheServer
{
    class CacheServer : ICacheService
    {
        DispatcherClient dc;
        ServiceHost host;

        //URL, Физический путь к файлу
        Dictionary<string, CachedFile> files = new Dictionary<string, CachedFile>();

        public CacheServer()
        {
            dc = new DispatcherClient(MetaData.Roles.cache);
            dc.onFound += dispFound;
            dc.StartListen();

            startService();

            if (!Directory.Exists(CachedFile.CacheFolder))
            {
                Directory.CreateDirectory(CachedFile.CacheFolder);
            }
        }

        //http://stackoverflow.com/questions/5907791/how-to-programatically-create-a-wcf-service-and-its-metadata-on-the-same-url
        void startService()
        {
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["cacheServicePort"].ToString());
            IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress compIP = adresses.First(x => x.AddressFamily == AddressFamily.InterNetwork);
            UriBuilder uriBuilder = new UriBuilder(compIP.ToString());
            uriBuilder.Port = port;
            Uri serviceUri = uriBuilder.Uri;

            BasicHttpBinding binding = new BasicHttpBinding();
            host = new ServiceHost(typeof(CacheServer), serviceUri);
            
            host.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true});
            host.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
            host.Description.Behaviors.Find<ServiceDebugBehavior>().HttpHelpPageUrl = serviceUri;

            host.AddServiceEndpoint(typeof(ICacheService), binding, string.Empty);

            host.Open();

            Console.WriteLine("Запущен веб-сервис для отправки cache по адресу " + serviceUri.AbsoluteUri);
            
        }

        public bool cacheFileExists(string URL)
        {
            return files.ContainsKey(URL);
        }

        public string getCachedFile(string URL)
        {
            return files[URL].ReadFile();
        }

        public void notifyReferation(string URL)
        {
            CachedFile file = new CachedFile();
            files.Add(URL, file);
        }

        //Действия, при нахождении диспетчера
        void dispFound()
        {
            dc.Register();
        }

    }
}
