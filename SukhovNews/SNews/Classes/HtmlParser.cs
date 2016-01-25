using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsQuery;
using System.Net;
using System.IO;

namespace Rss
{
    class HtmlParser
    {
        private string url;
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        private string html;
        public string Html
        {
            get { return html; }
            set { html = value; }
        }


        public HtmlParser(string targetUrl)
        {
            url = targetUrl;
            try
            {
                var promise = CQ.CreateFromUrl(url);
                var textFull = promise.Find("#article_full_text p");

                string fullTextString = textFull.Text();
                //WebClient wc = new WebClient();
                //wc.Encoding = Encoding.UTF8;
                //string html = wc.DownloadString("http://ria.ru/world/20160125/1364783109.html");
                //WebRequest wr = WebRequest.Create(url);
                //wr.Credentials = CredentialCache.DefaultNetworkCredentials;
                //HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                //if (response.StatusCode != HttpStatusCode.OK)
                //{
                //    throw new Exception(response.StatusDescription);
                //}
                //using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                //{
                //    html = sr.ReadToEnd();
                //}
                //response.Close();
                string n = "123";
            }
            catch
            {
                // what you gonna do? 
            }
        }

        public string getText()
        {
            //var dom 
            return "";
        }
        
    }
}
