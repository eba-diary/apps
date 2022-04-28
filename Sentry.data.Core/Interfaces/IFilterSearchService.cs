using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IFilterSearchService
    {
        string SaveSearch(SavedSearchDto savedSearchDto);
        SavedSearchDto GetSavedSearch(string searchType, string savedSearchName, string associateId);
        List<SavedSearchOptionDto> GetSavedSearchOptions(string searchType, string associateId);
    }
}
