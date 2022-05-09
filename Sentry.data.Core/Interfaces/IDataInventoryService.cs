using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDataInventoryService
    {
        DataInventorySearchResultDto GetSearchResults(FilterSearchDto dtoSearch);
        FilterSearchDto GetSearchFilters(FilterSearchDto dtoSearch);
        bool UpdateIsSensitive(List<DataInventoryUpdateDto> dtos);
        bool DoesItemContainSensitive(DataInventorySensitiveSearchDto dtoSearch);
        List<DataInventoryCategoryDto> GetCategoriesByAsset(string search);
        bool TryGetCategoryName(string category, out string categoryName);
    }
}
