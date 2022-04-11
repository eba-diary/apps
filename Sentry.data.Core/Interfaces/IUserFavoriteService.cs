using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IUserFavoriteService
    {
        IList<FavoriteItem> GetUserFavoriteItems(string associateId);
    }
}
