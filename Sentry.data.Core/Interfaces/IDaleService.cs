using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDaleService
    {
        DaleResultDto GetSearchResults(DaleSearchDto dtoSearch);
        bool UpdateIsSensitive(List<DaleSensitiveDto> dtos);

        DaleContainSensitiveResultDto DoesItemContainSensitive(DaleSearchDto dtoSearch);
    }
}
