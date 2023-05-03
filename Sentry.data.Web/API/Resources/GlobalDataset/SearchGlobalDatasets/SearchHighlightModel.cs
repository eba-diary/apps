using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class SearchHighlightModel
    {
        public string PropertyName { get; set; }
        public List<string> Highlights { get; set; }
    }
}