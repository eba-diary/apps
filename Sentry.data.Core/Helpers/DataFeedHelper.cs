using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Helpers
{
    public static class DataFeedHelper
    {
        public static string GetImage(string feedType)
        {
            string feedImg = "";

            switch (feedType)
            {
                case DataFeedConstants.FeedType.SAS:
                    feedImg = "/Images/sas_logo_min.png";
                    break;
                case DataFeedConstants.FeedType.Tab:
                    feedImg = "/Images/tableau-icon_min.png";
                    break;
                case DataFeedConstants.FeedType.Datasets:
                    feedImg = "/Images/Icons/Datasets.svg";
                    break;
                case DataFeedConstants.FeedType.DataAssets:
                    feedImg = "/Images/Icons/DataAssets.svg";
                    break;
                case DataFeedConstants.FeedType.Exhibits:
                    feedImg = "/Images/Icons/Business Intelligence.svg";
                    break;
            }

            return feedImg;
        }

        public static string GetUrl(string feedType, string feedId)
        {
            string feedUrl = "";

            switch (feedType)
            {
                case DataFeedConstants.FeedType.SAS:
                    feedUrl = feedId;
                    break;
                case DataFeedConstants.FeedType.Tab:
                    feedUrl = feedId;
                    break;
                case DataFeedConstants.FeedType.Datasets:
                    feedUrl = "/Dataset/Detail/" + feedId;
                    break;
                case DataFeedConstants.FeedType.DataAssets:
                    feedUrl = feedId;
                    break;
                case DataFeedConstants.FeedType.Exhibits:
                    feedUrl = "/BusinessIntelligence/Detail/" + feedId;
                    break;
            }

            return feedUrl;
        }
    }
}