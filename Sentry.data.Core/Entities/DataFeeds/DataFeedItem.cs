using System;
using Sentry.data.Core.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFeedItem
    {
        public DataFeedItem(DateTime pubDate, string shortDesc, string longDesc, DataFeed dataFeed)
        {
            PublishDate = pubDate;
            Title = shortDesc;
            Description = longDesc;
            Feed = dataFeed;
            Img = Helpers.DataFeedHelper.GetImage(dataFeed.Type);
            Url = Helpers.DataFeedHelper.GetUrl(dataFeed);
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

        public System.Web.HtmlString NotificationTitle
        {
            get
            {
                return new System.Web.HtmlString(System.Net.WebUtility.HtmlDecode(Title));
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