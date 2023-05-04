using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IGlobalDatasetSearchService
    {
        List<FilterCategoryDto> GetInitialFilters(List<string> filters);
        GlobalDatasetPageResultDto SetGlobalDatasetPageResults(GlobalDatasetPageRequestDto globalDatasetPageRequestDto);
    }
}
