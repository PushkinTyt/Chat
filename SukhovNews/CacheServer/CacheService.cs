using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServer
{
    class CacheService : ICacheService
    {
        public static Dictionary<string, CachedFile> files = new Dictionary<string, CachedFile>();

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
    }
}
