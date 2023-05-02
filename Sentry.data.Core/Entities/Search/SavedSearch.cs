using System.Web;

namespace Sentry.data.Core
{
    public class SavedSearch : IFavorable
    {
        public virtual int SavedSearchId { get; set; }
        public virtual string SearchType { get; set; }
        public virtual string SearchName { get; set; }
        public virtual string SearchText { get; set; }
        public virtual string FilterCategoriesJson { get; set; }
        public virtual string AssociateId { get; set; }
        public virtual string ResultConfigurationJson { get; set; }

        #region IFavorable
        public virtual void SetFavoriteItem(FavoriteItem favoriteItem)
        {
            favoriteItem.Title = SearchName;
            favoriteItem.Url = GetUrl();
            favoriteItem.Img = GetImgPath();
            favoriteItem.FeedUrlType = "WEB";
        }
        #endregion

        #region Methods
        public virtual string GetUrl()
        {
            string url = "";
            switch (SearchType)
            {
                case GlobalConstants.SearchType.DATA_INVENTORY:
                    url = "DataInventory/Search";
                    break;
                case GlobalConstants.SearchType.DATASET_SEARCH:
                case GlobalConstants.SearchType.GLOBAL_DATASET:
                    url = "Search/Datasets";
                    break;
            }

            return $"{url}?savedSearch={HttpUtility.UrlEncode(SearchName)}";
        }
        
        private string GetImgPath()
        {
            switch (SearchType)
            {
                case GlobalConstants.SearchType.DATA_INVENTORY:
                    return "/Images/DataInventory/DataInventoryIcon.svg";
                case GlobalConstants.SearchType.DATASET_SEARCH:
                case GlobalConstants.SearchType.GLOBAL_DATASET:
                    return "/Images/Icons/search_icon.svg";
                default:
                    return "";
            }
        }
        #endregion
    }
}
