using System.Collections.Generic;
using System.Linq;
using Sentry.data.Core;
using Sentry.Core;

namespace Sentry.data.Infrastructure
{
    public static class RetrieverJobQueryExtensions
    {
        public static List<RetrieverJob> FetchAllConfiguration(this IQueryable<RetrieverJob> query, IDatasetContext datasetContext)
        {
            /* Retrieve Config(s) and Schema(s) */
            
            var configs = datasetContext.DatasetFileConfigs.Where(x => query.Any(y => x.ConfigId == y.DatasetConfig.ConfigId));
            var schemas = datasetContext.DataElements.Where(x => configs.Any(y => x.DatasetFileConfig.ConfigId == y.ConfigId));
            schemas.FetchMany(x => x.DataElementDetails).ToFuture();
            schemas.FetchMany(x => x.DataObjects).ToFuture();
            query.Fetch(x => x.DatasetConfig).ThenFetchMany(x => x.Schema).ToFuture();
            
            var jobs = query.Fetch(f => f.DatasetConfig).ThenFetch(x => x.ParentDataset).Fetch(f => f.DataSource).ToFuture();

            return jobs.ToList();
        }
    }
}
