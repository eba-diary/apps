using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class SavedSearchesModel
    {
        public string SearchType { get; set; }
        public List<string> SavedSearchNames { get; set; }
        public string ActiveSearchName { get; set; }
    }
}