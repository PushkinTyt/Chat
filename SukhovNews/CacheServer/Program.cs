using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;

namespace CacheServer
{
    class Program
    {
        private static ManualResetEvent _ResetEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            Thread serviceThread = new Thread(startService);
            serviceThread.Start();
            CacheServer cs = new CacheServer();
            Console.ReadKey();
        }

        //http://stackoverflow.com/questions/5907791/how-to-programatically-create-a-wcf-service-and-its-metadata-on-the-same-url
        static void startService()
        {
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["cacheServicePort"].ToString());
            IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress compIP = adresses.First(x => x.AddressFamily == AddressFamily.InterNetwork);
            UriBuilder uriBuilder = new UriBuilder(compIP.ToString());
            uriBuilder.Port = port;
            Uri serviceUri = uriBuilder.Uri;

            BasicHttpBinding binding = new BasicHttpBinding();
            var host = new ServiceHost(typeof(CacheService), serviceUri);

            host.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true });
            host.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
            host.Description.Behaviors.Find<ServiceDebugBehavior>().HttpHelpPageUrl = serviceUri;

            host.AddServiceEndpoint(typeof(ICacheService), binding, string.Empty);

            try
            {
                host.Open();
                Console.WriteLine("Запущен веб-сервис cache по адресу " + serviceUri.AbsoluteUri);
            }
            catch
            {
                Console.WriteLine("Не удалось запустить веб-сервис (вероятно, приложение запущено без администраторских прав). ");
            }
            
        }
    }
}
