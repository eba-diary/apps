using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IUserFavoriteService
    {
        IList<FavoriteItem> GetUserFavoriteItems(string associateId);
        void RemoveUserFavorite(int userFavoriteId, bool isLegacyFavorite);
        IList<FavoriteItem> SetUserFavoritesOrder(List<KeyValuePair<int, bool>> orderedIds);
        void AddUserFavorite(string favoriteType, int entityId, string associateId);
        UserFavorite GetUserFavoriteByEntity(int entityId, string associateId);
    }
}
