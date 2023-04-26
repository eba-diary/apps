using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IGlobalDatasetService
    {
        Task<SearchGlobalDatasetsResultsDto> SearchGlobalDatasetsAsync(SearchGlobalDatasetsDto searchGlobalDatasetsDto);
        Task<GetGlobalDatasetFiltersResultDto> GetGlobalDatasetFiltersAsync(GetGlobalDatasetFiltersDto getGlobalDatasetFiltersDto);
    }
}
