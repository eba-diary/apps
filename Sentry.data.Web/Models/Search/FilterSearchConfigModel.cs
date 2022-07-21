using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class FilterSearchConfigModel
    {
        public string IconPath { get; set; }
        public string PageTitle { get; set; }
        public string SearchType { get; set; }
        public string InfoLink { get; set; }
        public FilterSearchModel DefaultSearch { get; set; }
    }
}