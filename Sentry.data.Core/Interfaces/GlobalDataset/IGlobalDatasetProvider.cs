using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IGlobalDatasetProvider
    {
        Task<List<GlobalDataset>> SearchGlobalDatasetsAsync(BaseFilterSearchDto filterSearchDto);
        Task<List<FilterCategoryDto>> GetGlobalDatasetFiltersAsync(BaseFilterSearchDto filterSearchDto);
        Task<DocumentsFiltersDto<GlobalDataset>> GetGlobalDatasetsAndFiltersAsync(BaseFilterSearchDto filterSearchDto);
        Task<List<GlobalDataset>> GetGlobalDatasetsByEnvironmentDatasetIdsAsync(List<int> environmentDatasetIds);
        Task<List<FilterCategoryDto>> GetFiltersByEnvironmentDatasetIdsAsync(List<int> environmentDatasetIds);
        Task AddUpdateGlobalDatasetAsync(GlobalDataset globalDataset);
        Task AddUpdateGlobalDatasetsAsync(List<GlobalDataset> globalDatasets);
        Task DeleteGlobalDatasetsAsync(List<int> globalDatasetIds);
        Task AddUpdateEnvironmentDatasetAsync(int globalDatasetId, EnvironmentDataset environmentDataset);
        Task DeleteEnvironmentDatasetAsync(int environmentDatasetId);
        Task AddEnvironmentDatasetFavoriteUserIdAsync(int environmentDatasetId, string favoriteUserId);
        Task RemoveEnvironmentDatasetFavoriteUserIdAsync(int environmentDatasetId, string favoriteUserId, bool removeForAllEnvironments);
        Task AddUpdateEnvironmentSchemaAsync(int environmentDatasetId, EnvironmentSchema environmentSchema);
        Task AddUpdateEnvironmentSchemaSaidAssetCodeAsync(int environmentSchemaId, string saidAssetCode);
        Task DeleteEnvironmentSchemaAsync(int environmentSchemaId);
    }
}