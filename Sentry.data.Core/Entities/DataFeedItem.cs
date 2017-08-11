using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFeedItem
    {
        private string title;
        private DateTime publishDate;
        private string url;
        private string description;
        private DataFeed feed;
        private string img;

        public DataFeedItem(DateTime pubDate, string id, string shortDesc, string longDesc, DataFeed type)
        {
            publishDate = pubDate;
            url = id;
            title = shortDesc;
            description = longDesc;
            feed = type;

            switch (feed.Type)
            {
                case "SAS":
                    img = "/Images/sas_logo_min.png";
                    break;
                case "TAB":
                    img = "/Images/tableau-icon_min.png";
                    break;
            }
        }

        public string DisplayTitle()
        {
            if (title.Length > 50)
            {
                return title.Substring(0, 50).Trim() + "...";
            }
            else
            {
                return title;
            }
        }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
            }
        }

        public DateTime PublishDate
        {
            get
            {
                return publishDate;
            }

            set
            {
                publishDate = value;
            }
        }

        public string Url
        {
            get
            {
                return url;
            }

            set
            {
                url = value;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
            }
        }

        public DataFeed Feed
        {
            get
            {
                return feed;
            }

            set
            {
                feed = value;
            }
        }

        public string Img
        {
            get
            {
                return img;
            }

            set
            {
                img = value;
            }
        }
    }
}
