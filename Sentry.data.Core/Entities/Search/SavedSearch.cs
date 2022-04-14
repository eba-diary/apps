using System.Web;

namespace Sentry.data.Core
{
    public class SavedSearch : IFavorable
    {
        public virtual int SavedSearchId { get; set; }
        public virtual string SearchName { get; set; }
        public virtual string SearchText { get; set; }
        public virtual string FilterCategoriesJson { get; set; }
        public virtual string AssociateId { get; set; }

        #region IFavorable
        public virtual int GetFavoriteEntityId()
        {
            return SavedSearchId;
        }

        public virtual string GetFavoriteType()
        {
            return GlobalConstants.UserFavoriteTypes.SAVEDSEARCH;
        }

        public virtual void SetFavoriteItem(FavoriteItem favoriteItem)
        {
            favoriteItem.Title = SearchName;
            favoriteItem.Url = $"DataInventory/Search?savedSearch={HttpUtility.UrlEncode(SearchName)}";
            favoriteItem.Img = "/Images/DataInventory/DataInventoryIcon.png";
        }
        #endregion
    }
}
