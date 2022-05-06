
namespace Sentry.data.Core.Helpers
{
    public static class DataFeedHelper
    {
        public static string GetImage(string type)
        {
            switch (type)
            {
                case GlobalConstants.DataFeedType.SAS:
                    return "/Images/sas_logo_min.png";
                case GlobalConstants.DataFeedType.Tab:
                    return "/Images/tableau-icon_min.png";
                case GlobalConstants.DataFeedType.Datasets:
                    return "/Images/Icons/Datasets.svg";
                case GlobalConstants.DataFeedType.DataAssets:
                    return "/Images/Icons/DataAssets.svg";
                case GlobalConstants.DataFeedType.Exhibits:
                    return "/Images/Icons/Business Intelligence.svg";
                case GlobalConstants.DataFeedType.Notifications:
                    return "/Images/Icons/Blogs.png";
                case GlobalConstants.DataFeedType.Schemas:
                    return "/Images/Icons/Metadata.png";
                default:
                    return "/Images/Icons/Metadata.png";
            }
        }

        public static string GetUrl(DataFeed df)
        {
            switch (df.Type)
            {
                case GlobalConstants.DataFeedType.SAS:
                case GlobalConstants.DataFeedType.Tab:
                case GlobalConstants.DataFeedType.DataAssets:
                    return df.Id.ToString();
                case GlobalConstants.DataFeedType.Datasets:
                    return $"/Dataset/Detail/{df.Id}";
                case GlobalConstants.DataFeedType.Exhibits:
                    return $"/BusinessIntelligence/Detail/{df.Id}";
                case GlobalConstants.DataFeedType.Schemas:
                    return $"/Dataset/Detail/{df.Id}?configID={df.Id2}";
                default:
                    return string.Empty;
            }
        }
    }
}