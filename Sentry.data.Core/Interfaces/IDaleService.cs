using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDaleService
    {
        DaleResultsDto GetSearchResults(DaleSearchDto dto);
    }
}
