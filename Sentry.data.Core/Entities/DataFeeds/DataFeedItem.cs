using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFeedItem
    {
        public DataFeedItem(DateTime pubDate, string id, string shortDesc, string longDesc, DataFeed type)
        {
            PublishDate = pubDate;
            Title = shortDesc;
            Description = longDesc;
            Feed = type;

            switch (Feed.Type)
            {
                case "SAS":
                    Img = "/Images/sas_logo_min.png";
                    Url = id;
                    break;
                case "TAB":
                    Img = "/Images/tableau-icon_min.png";
                    Url = id;
                    break;
                case "Datasets":
                    Img = "/Images/Icons/Datasets.svg";
                    Url = "/Dataset/Detail/" + id;
                    break;
                case "Data Assets":
                    Img = "/Images/Icons/DataAssets.svg";
                    Url = id;
                    break;
                case "Exhibits":
                    Img = "/Images/Icons/Business Intelligence.svg";
                    Url = "/Report";
                    break;
            }
        }

        public string DisplayTitle()
        {
            if (Title.Length > 100)
            {
                return Title.Substring(0, 100).Trim() + "...";
            }
            else
            {
                return Title;
            }
        }

        public string Title { get; set; }
        public DateTime PublishDate { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DataFeed Feed { get; set; }
        public string Img { get; set; }
    }
}
