using Nest;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public abstract class SearchHighlightable
    {
        [Ignore]
        public List<SearchHighlight> SearchHighlights { get; set;  }
    }
}
