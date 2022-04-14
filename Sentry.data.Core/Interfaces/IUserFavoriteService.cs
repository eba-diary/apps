using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IUserFavoriteService
    {
        IList<FavoriteItem> GetUserFavoriteItems(string associateId);
        void RemoveUserFavorite(int userFavoriteId, string associateId);
        IList<FavoriteItem> SetUserFavoritesOrder(List<int> orderedIds, string associateId);
        void AddUserFavorite(IFavorable favorite, string associateId);
    }
}
