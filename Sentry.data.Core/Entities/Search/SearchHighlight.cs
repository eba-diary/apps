using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class SearchHighlight
    {
        public string PropertyName { get; set; }
        public List<string> Highlights { get; set; }
    }
}
