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
            favoriteItem.Url = $"{GetUrl()}?savedSearch={HttpUtility.UrlEncode(SearchName)}";
            favoriteItem.Img = GetImgPath();
            favoriteItem.FeedUrlType = "WEB";
        }
        #endregion

        #region Methods
        private string GetUrl()
        {
            switch (SearchType)
            {
                case GlobalConstants.SearchType.DATA_INVENTORY:
                    return "DataInventory/Search";
                default:
                    return "";
            }
        }
        
        private string GetImgPath()
        {
            switch (SearchType)
            {
                case GlobalConstants.SearchType.DATA_INVENTORY:
                    return "/Images/DataInventory/DataInventoryIcon.svg";
                default:
                    return "";
            }
        }
        #endregion
    }
}
