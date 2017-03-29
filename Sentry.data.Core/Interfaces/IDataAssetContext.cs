using Sentry.Core;
using System.Linq;

namespace Sentry.data.Core
{
    public interface IDataAssetContext : IWritableDomainContext
    {
        IQueryable<DomainUser> Users { get; }
        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        IQueryable<Asset> Assets { get; }
        IQueryable<Category> Categories { get; }

        void DeleteAllData();
        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
    }

}
