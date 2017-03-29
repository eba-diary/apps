using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFeedItem
    {
        public DataFeedItem(DateTime publishDate, string url, string shortDesc, string longDesc)
        {
            PublishDate = publishDate;
            URL = url;
            ShortDescription = shortDesc;
            LongDescription = longDesc;
        }

        public DateTime PublishDate { get; set; }
        public string URL { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }

    }
}
