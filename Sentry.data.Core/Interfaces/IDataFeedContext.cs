using Sentry.Core;
using System.Linq;

namespace Sentry.data.Core
{
    public interface IDataFeedContext : IReadableStatelessDomainContext
    {
        IQueryable<DataFeedItem> HotTopicsFeed { get; }
        IQueryable<DataFeedItem> NewsFeed { get; }
    }

}
