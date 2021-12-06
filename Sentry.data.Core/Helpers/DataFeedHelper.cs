
namespace Sentry.data.Core.Helpers
{
    public static class DataFeedHelper
    {
        public static string GetImage(string feedType)
        {
            string feedImg = "";

            switch (feedType)
            {
                case GlobalConstants.DataFeedType.SAS:
                    feedImg = "/Images/sas_logo_min.png";
                    break;
                case GlobalConstants.DataFeedType.Tab:
                    feedImg = "/Images/tableau-icon_min.png";
                    break;
                case GlobalConstants.DataFeedType.Datasets:
                    feedImg = "/Images/Icons/Datasets.svg";
                    break;
                case GlobalConstants.DataFeedType.DataAssets:
                    feedImg = "/Images/Icons/DataAssets.svg";
                    break;
                case GlobalConstants.DataFeedType.Exhibits:
                    feedImg = "/Images/Icons/Business Intelligence.svg";
                    break;
                case GlobalConstants.DataFeedType.Notifications:
                    feedImg = "/Images/Icons/Blogs.png";
                    break;
            }

            return feedImg;
        }

        public static string GetUrl(string feedType, string feedId)
        {
            string feedUrl = "";

            switch (feedType)
            {
                case GlobalConstants.DataFeedType.SAS:
                    feedUrl = feedId;
                    break;
                case GlobalConstants.DataFeedType.Tab:
                    feedUrl = feedId;
                    break;
                case GlobalConstants.DataFeedType.Datasets:
                    feedUrl = "/Dataset/Detail/" + feedId;
                    break;
                case GlobalConstants.DataFeedType.DataAssets:
                    feedUrl = feedId;
                    break;
                case GlobalConstants.DataFeedType.Exhibits:
                    feedUrl = "/BusinessIntelligence/Detail/" + feedId;
                    break;
            }

            return feedUrl;
        }
    }
}