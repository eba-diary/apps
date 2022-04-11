using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IUserFavoriteRepository
    {
        IList<UserFavorite> GetUserFavorites(string associateId);
    }
}
