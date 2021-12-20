
namespace Sentry.data.Core.Helpers
{
    public static class DataFeedHelper
    {
        public static string GetImage(DataFeed df)
        {
            string feedImg = "";

            switch (df.Type)
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
                case GlobalConstants.DataFeedType.Schemas:
                    feedImg = "/Images/Icons/Metadata.png";
                    break;
                default:
                    feedImg = "/Images/Icons/Metadata.png";
                    break;
            }

            return feedImg;
        }

        public static string GetUrl(DataFeed df)
        {
            string feedUrl = "";

            switch (df.Type)
            {
                case GlobalConstants.DataFeedType.SAS:
                    feedUrl = df.Id.ToString();
                    break;
                case GlobalConstants.DataFeedType.Tab:
                    feedUrl = df.Id.ToString();
                    break;
                case GlobalConstants.DataFeedType.Datasets:
                    feedUrl = "/Dataset/Detail/" + df.Id.ToString();
                    break;
                case GlobalConstants.DataFeedType.DataAssets:
                    feedUrl = df.Id.ToString();
                    break;
                case GlobalConstants.DataFeedType.Exhibits:
                    feedUrl = "/BusinessIntelligence/Detail/" + df.Id.ToString();
                    break;
                case GlobalConstants.DataFeedType.Schemas:
                    feedUrl = "/Dataset/Detail/" + df.Id.ToString() + "?configID=" + df.Id2.ToString();
                    break;
                default:
                    feedUrl = string.Empty;
                    break;
            }

            return feedUrl;
        }
    }
}