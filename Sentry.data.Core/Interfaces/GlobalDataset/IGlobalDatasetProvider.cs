using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IGlobalDatasetProvider
    {
        Task AddUpdateGlobalDatasetAsync(GlobalDataset globalDataset);
        Task AddUpdateEnvironmentDatasetAsync(int globalDatasetId, EnvironmentDataset environmentDataset);
        Task DeleteEnvironmentDatasetAsync(int environmentDatasetId);
        Task AddUpdateEnvironmentSchemaAsync(int environmentDatasetId, EnvironmentSchema environmentSchema);
        Task DeleteEnvironmentSchemaAsync(int environmentSchemaId);
    }
}
