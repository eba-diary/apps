﻿using Sentry.Core;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public interface IDataFeedContext : IReadableStatelessDomainContext
    {
        IList<DataFeedItem> GetAllFeedItems();
        IList<DataFeedItem> GetSentryFeedItems();
        IList<DataFeedItem> GoGetItems(List<DataFeed> dataFeeds);
        IList<FavoriteItem> GetUserFavorites(string associateId);
        FavoriteItem GetFavorite(int favoriteId);
    }

}