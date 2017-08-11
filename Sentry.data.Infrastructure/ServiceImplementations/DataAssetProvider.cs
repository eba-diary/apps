using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;
using Sentry.NHibernate;
using NHibernate;
using NHibernate.Linq;

namespace Sentry.data.Infrastructure
{
    public class DataAssetProvider : NHWritableDomainContext, IDataAssetProvider
    {
        //ICLPFactory factory = new CLPFactory();
        //IConsumptionLayerProvider clp;

        public DataAssetProvider(ISession session) : base(session)
        {
            NHQueryableExtensionProvider.RegisterQueryableExtensionsProvider<DataAssetProvider>();
        }

        public DataAsset GetDataAsset(int id)
        {
            DataAsset da = Query<DataAsset>().Cacheable().Where(x => x.Id == id).FetchMany(x => x.Components).ToList().FirstOrDefault();

            return da;
        }

        public DataAsset GetDataAsset(string assetName)
        {
            DataAsset da = Query<DataAsset>().Cacheable().Where(x => x.Name == assetName).FetchMany(x => x.Components).ToList().FirstOrDefault();

            return da;
        }

        public IList<DataAsset> GetDataAssets()
        {
            return Query<DataAsset>().Cacheable().OrderBy(x => x.Name).ToList();
        }
    }
}
