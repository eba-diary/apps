using System.Linq;
using NHibernate;
using NHibernate.Linq;
using Sentry.Core;
using Sentry.NHibernate;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    public class dataAssetContext : NHWritableDomainContext, IDataAssetContext
    {
        public dataAssetContext(ISession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<dataAssetContext>();
        }

        public IQueryable<DomainUser> Users
        {
            get
            {
                //return Query<DomainUser>();
                return null;
            }
        }

        //###  BEGIN Sentry.Data  A### - Code below is Sentry.Data-specific
        public IQueryable<Asset> Assets
        {
            get
            {
                //return Query<Asset>();
                return null;
            }
        }

        public IQueryable<Category> Categories
        {
            get
            {
                //return Query<Category>().Cacheable(QueryCacheRegion.MediumTerm);
                return null;
            }
        }

        public void DeleteAllData()
        {
            DemoDataService.DeleteAllDemoData(this.Session);
        }
        //###  END Sentry.Data  ### - Code above is Sentry.Data-specific
    }
}
