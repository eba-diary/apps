using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IGlobalDatasetProvider
    {
        Task AddUpdateGlobalDatasetAsync(GlobalDataset globalDataset);
        Task AddUpdateGlobalDatasetsAsync(List<GlobalDataset> globalDatasets);
        Task DeleteGlobalDatasetsAsync(List<int> globalDatasetIds);
        Task AddUpdateEnvironmentDatasetAsync(int globalDatasetId, EnvironmentDataset environmentDataset);
        Task DeleteEnvironmentDatasetAsync(int environmentDatasetId);
        Task AddEnvironmentDatasetFavoriteUserIdAsync(int environmentDatasetId, string favoriteUserId);
        Task RemoveEnvironmentDatasetFavoriteUserIdAsync(int environmentDatasetId, string favoriteUserId);
        Task AddUpdateEnvironmentSchemaAsync(int environmentDatasetId, EnvironmentSchema environmentSchema);
        Task AddUpdateEnvironmentSchemaSaidAssetCodeAsync(int environmentSchemaId, string saidAssetCode);
        Task DeleteEnvironmentSchemaAsync(int environmentSchemaId);
    }
}
