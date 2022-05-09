using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDataInventorySearchProvider
    {
        DataInventorySearchResultDto GetSearchResults(FilterSearchDto dto);
        FilterSearchDto GetSearchFilters(FilterSearchDto dto);
        bool SaveSensitive(List<DataInventoryUpdateDto> dtos);
        DataInventorySensitiveSearchResultDto DoesItemContainSensitive(DataInventorySensitiveSearchDto dto);
        DataInventoryAssetCategoriesDto GetCategoriesByAsset(string search);
    }
}
