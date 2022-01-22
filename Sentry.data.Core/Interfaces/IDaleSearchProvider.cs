using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDaleSearchProvider
    {
        DaleResultDto GetSearchResults(DaleSearchDto dto);
        List<FilterCategoryDto> GetSearchFilters(DaleSearchDto dto);
        bool SaveSensitive(string sensitiveBlob);
        DaleContainSensitiveResultDto DoesItemContainSensitive(DaleSearchDto dto);
        DaleCategoryResultDto GetCategoriesByAsset(string search);
    }
}
