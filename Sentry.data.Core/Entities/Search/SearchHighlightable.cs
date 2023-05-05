using Nest;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public abstract class SearchHighlightable
    {
        [Ignore]
        public List<SearchHighlight> SearchHighlights { get; set;  }

        public void AddSearchHighlight(string propertyName, string highlight)
        {
            if (SearchHighlights == null)
            {
                SearchHighlights = new List<SearchHighlight>();
            }

            SearchHighlight searchHighlight = SearchHighlights.FirstOrDefault(x => x.PropertyName == propertyName);

            if (searchHighlight == null)
            {
                searchHighlight = new SearchHighlight { PropertyName = propertyName };
                searchHighlight.AddHighlight(highlight);

                SearchHighlights.Add(searchHighlight);
            }
            else
            {
                searchHighlight.AddHighlight(highlight);
            }
        }
    }
}
