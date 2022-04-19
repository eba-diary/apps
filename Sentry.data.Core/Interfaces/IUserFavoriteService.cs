using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IUserFavoriteService
    {
        IList<FavoriteItem> GetUserFavoriteItems(string associateId);
        void RemoveUserFavorite(int userFavoriteId, bool isLegacyFavorite);
        IList<FavoriteItem> SetUserFavoritesOrder(List<KeyValuePair<int, bool>> orderedIds);
        void AddUserFavorite(IFavorable favorite, string associateId);
    }
}
