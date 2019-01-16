using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;

namespace Sentry.data.Core
{
    public class FavoriteItem
    {
        public FavoriteItem(string id, string title, DataFeed feed, int sequence)
        {
            Title = title;
            Sequence = sequence;
            FeedName = feed.Name;
            FeedUrlType = feed.UrlType;
            FeedUrl = feed.Url;
            FeedId = feed.Id;

            Img = Helpers.DataFeedHelper.GetImage(feed.Type);
            Url = Helpers.DataFeedHelper.GetUrl(feed.Type, feed.Id.ToString());
        }

        public string Title { get; set; }
        public string Url { get; set; }
        public int Sequence { get; set; }
        public string Img { get; set; }
        public string FeedName { get; set; }
        public string FeedUrlType { get; set; }
        public string FeedUrl { get; set; }
        public int FeedId { get; set; }
    }
}