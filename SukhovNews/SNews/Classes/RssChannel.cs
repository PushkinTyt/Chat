using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Xml;
using System.Diagnostics;

namespace Rss
{
    [Serializable]
    class RssChannel
    {
        private string url;
        private string category;

        public string Category
        {
            get { return category; }
            set { category = value; }
        }

        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        public List<RssItem> Articles;

        public RssChannel(string channelAdress, string categ)
        {
            url = channelAdress;
            Articles = new List<RssItem>();
            category = categ;
            //downloadRssItems();
        }

        public void Update()
        {
            try
            {
                WebRequest wr = WebRequest.Create(this.url);
                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("при загрузке rss произошла ошибка (status != 200)");
                }
                XmlReader xr = XmlReader.Create(response.GetResponseStream());
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(xr);
                XmlNode root = xDoc.DocumentElement;
                XmlNodeList items = root.SelectNodes("channel/item");
                //Debug.Print(root.InnerText);
                string m = root.InnerXml;
                foreach (XmlNode item in items)
                {
                    Articles.Add(new RssItem
                    {
                        Description = item.SelectSingleNode("description").InnerText,
                        Title = item.SelectSingleNode("title").InnerText,
                        link = item.SelectSingleNode("link").InnerText,
                        PubDate = DateTime.Parse(item.SelectSingleNode("pubDate").InnerText),
                        rssChannel = this
                    });
                }
            }
            catch
            {
                throw new Exception("не удалось загрузить статьи");
            }
        }
    }
}
