using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rss
{
    [Serializable]
    class RssItem
    {
        public RssChannel rssChannel; // ссылка на родителя (канал rss)
        
        // Заголовок статьи
        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        // описание статьи
        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        
        // ссылка на статью
        public string link;

        // дата публикации статьи
        private DateTime pubDate;
        public DateTime PubDate
        {
            get { return pubDate; }
            set { pubDate = value; }
        }
    }
}
