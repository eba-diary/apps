using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IDaleService
    {
        Task<DaleResultDto> GetSearchResults(DaleSearchDto dtoSearch);
        FilterSearchDto GetSearchFilters(DaleSearchDto dtoSearch);
        bool UpdateIsSensitive(List<DaleSensitiveDto> dtos);
        DaleContainSensitiveResultDto DoesItemContainSensitive(DaleSearchDto dtoSearch);
        DaleCategoryResultDto GetCategoriesByAsset(string search);
        bool TryGetCategoryName(string category, out string categoryName);
    }
}
