using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class FilterSearchConfigModel
    {
        public string PageTitle { get; set; }
        public string SearchType { get; set; }
        public FilterSearchModel DefaultSearch { get; set; }
        public Dictionary<string, string> ResultParameters { get; set; }
        public bool HasSearchSettings { get; set; }
    }
}