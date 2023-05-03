using System.Collections.Generic;

namespace Sentry.data.Core
{
    public abstract class SearchHighlightableDto
    {
        public List<SearchHighlightDto> SearchHighlights { get; set; }
    }
}
