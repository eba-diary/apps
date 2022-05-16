using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class SavedSearchDropdownModel
    {
        public string SearchType { get; set; }
        public List<SavedSearchOptionModel> SavedSearchOptions { get; set; }
        public string ActiveSearchName { get; set; }
    }
}