using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDaleService
    {
        List<DaleResultDto> GetSearchResults(DaleSearchDto dto);
        bool UpdateIsSensitive(List<DaleSensitiveDto> dtos);
    }
}
