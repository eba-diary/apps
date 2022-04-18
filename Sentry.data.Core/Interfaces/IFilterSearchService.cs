using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IFilterSearchService
    {
        void SaveSearch(SavedSearchDto savedSearchDto);
        SavedSearchDto GetSavedSearch(string searchType, string savedSearchName, string associateId);
        List<string> GetSavedSearchNames(string searchType, string associateId);
    }
}
