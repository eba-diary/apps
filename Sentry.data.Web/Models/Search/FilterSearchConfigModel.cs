using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class FilterSearchConfigModel
    {
        public string IconPath { get; set; }
        public string PageTitle { get; set; }
        public string ResultView { get; set; }
        public string InfoLink { get; set; }
        public FilterSearchModel DefaultSearch { get; set; }
        public List<string> SavedSearchNames { get; set; }
    }
}