using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class SearchableDatasetProvider : ISearchableDatasetProvider
    {
        public Task AddSearchableDatasetAsync(SearchableDataset searchableDataset)
        {
            throw new NotImplementedException();
        }

        public Task DeleteSearchableDatasetAsync(SearchableDataset searchableDataset)
        {
            throw new NotImplementedException();
        }

        public Task<List<SearchableDataset>> GetSearchableDatasetsAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateSearchableDatasetAsync(SearchableDataset searchableDataset)
        {
            throw new NotImplementedException();
        }
    }
}
