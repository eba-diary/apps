using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface ISearchableDatasetProvider
    {
        Task<List<SearchableDataset>> GetSearchableDatasetsAsync();
        Task AddSearchableDatasetAsync(SearchableDataset searchableDataset);
        Task UpdateSearchableDatasetAsync(SearchableDataset searchableDataset);
        Task DeleteSearchableDatasetAsync(SearchableDataset searchableDataset);
    }
}
