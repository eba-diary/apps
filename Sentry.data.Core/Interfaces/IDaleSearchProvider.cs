using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IDaleSearchProvider
    {
        DaleResultDto GetSearchResults(DaleSearchDto dto);
        FilterSearchDto GetSearchFilters(DaleSearchDto dto);
        bool SaveSensitive(List<DaleSensitiveDto> dtos);
        DaleContainSensitiveResultDto DoesItemContainSensitive(DaleSearchDto dto);
        DaleCategoryResultDto GetCategoriesByAsset(string search);
    }
}
