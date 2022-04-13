using System.Web;

namespace Sentry.data.Core
{
    public class SavedSearch : IFavorable
    {
        public virtual int SavedSearchId { get; set; }
        public virtual string SearchName { get; set; }
        public virtual string SearchText { get; set; }
        public virtual string FilterCategoriesJson { get; set; }

        public FavoriteItem CreateFavoriteItem(UserFavorite userFavorite)
        {
            return new FavoriteItem()
            {
                Id = userFavorite.UserFavoriteId,
                Title = SearchName,
                Sequence = userFavorite.Sequence,
                Url = $"DataInventory/Search?savedSearch={HttpUtility.UrlEncode(SearchName)}",
                Img = "/Images/DataInventory/DataInventoryIcon.png"
            };
        }
    }
}
