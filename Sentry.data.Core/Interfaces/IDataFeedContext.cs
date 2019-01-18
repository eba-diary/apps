using Sentry.Core;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public interface IDataFeedContext : IReadableStatelessDomainContext
    {
        IList<DataFeed> GetDataFeeds();
        IList<DataFeedItem> GetAllFeedItems();
        IList<DataFeed> GetSentryDataFeeds();
        IList<DataFeedItem> GetSentryFeedItems();
        IList<DataFeedItem> GoGetItems(List<DataFeed> dataFeeds);
        IList<DataFeedItem> GetFeedItems(DataFeed feed);
        IList<FavoriteItem> GetUserFavorites(string associateId);
        FavoriteItem GetFavorite(int favoriteId);
    }

}