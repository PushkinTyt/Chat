using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Threading;

namespace CacheServer
{
    class CachedFile
    {
        static string cacheFolder = ConfigurationManager.AppSettings["cacheFolder"].ToString();
        string path;

        bool ready = false;

        public static string CacheFolder
        {
            get
            {
                return cacheFolder;
            }
        }

        public void WriteFile(string URL, string xmlFile)
        {
            char[] illegalChars = Path.GetInvalidFileNameChars();
            foreach (char ilChar in illegalChars)
            {
                URL = URL.Replace(ilChar, '\0');
            }

            string path = CacheFolder + "\\" + URL + ".xml";
            File.WriteAllText(path, xmlFile);

            ready = true;
        }

        public string ReadFile()
        {
            while(!ready)
            {
                Thread.Sleep(2000);
            }
            return File.ReadAllText(path);
        }
    }
}
