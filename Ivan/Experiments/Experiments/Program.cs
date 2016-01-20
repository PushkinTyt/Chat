using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Xml;
using System.Diagnostics;

namespace Experiments
{
    class Program
    {
        static void Main(string[] args)
        {
            RssChanel politicChannel = new RssChanel("http://ria.ru/export/rss2/politics/index.xml");

            try
            {
                WebRequest wr = WebRequest.Create(politicChannel.Url);
                // если потребуются данные пользователя
                //wr.Credentials = CredentialCache.DefaultCredentials;

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Console.WriteLine("status: {0} - {1}", response.StatusCode, response.StatusDescription);

                XmlReader xr = XmlReader.Create(response.GetResponseStream());
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(xr);

                XmlNode root = xDoc.DocumentElement;
                XmlNodeList items = root.SelectNodes("channel/item");
                //Console.WriteLine(root.InnerText);
                foreach (XmlNode item in items)
                {
                    Console.WriteLine(item.SelectSingleNode("title").InnerText);
                }

                //XmlNodeList xnl = xDoc.SelectNodes()
                //Debug.Print(root.OuterXml);
                //Console.WriteLine(root.OuterXml);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
            }

            Console.ReadKey();
        } 
    }


    class RssItem
    {
        private RssChanel rssChannel;

        public string title;
        public string description;
        public string link;
        public string pubDate;
    }

    class RssChanel
    {
        private string url;
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        public List<RssItem> Articles;

        public RssChanel(string channelAdress)
        {
            url = channelAdress;
            Articles = new List<RssItem>();
        }

    }

}
