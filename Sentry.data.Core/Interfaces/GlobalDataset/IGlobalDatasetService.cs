using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IGlobalDatasetService
    {
        Task<SearchGlobalDatasetsResultDto> SearchGlobalDatasetsAsync(SearchGlobalDatasetsDto searchGlobalDatasetsDto);
        Task<List<FilterCategoryDto>> GetGlobalDatasetFiltersAsync(FilterSearchDto filterSearchDto);
    }
}
