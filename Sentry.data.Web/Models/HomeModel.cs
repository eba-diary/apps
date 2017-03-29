using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class HomeModel
    {
        public HomeModel()
        {

        }
        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        public List<BaseCategoryModel> RootCategories;
        public List<BaseAssetModel> RootAssets;
        public List<DataFeedItemModel> RootNewsFeedItems;
        public List<DataFeedItemModel> RootHotTopicFeedItems;
        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
    }
}
