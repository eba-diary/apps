using Nest;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public abstract class SearchHighlightable
    {
        [Ignore]
        public List<SearchHighlight> SearchHighlights { get; set;  }

        public void MergeSearchHighlights(List<SearchHighlight> mergeSearchHighlights)
        {
            if (SearchHighlights == null)
            {
                SearchHighlights = new List<SearchHighlight>();
            }

            foreach (SearchHighlight searchHighlight in mergeSearchHighlights)
            {
                SearchHighlight existingSearchHighlight = SearchHighlights.FirstOrDefault(x => x.PropertyName == searchHighlight.PropertyName);

                if (existingSearchHighlight == null)
                {
                    existingSearchHighlight = new SearchHighlight { PropertyName = searchHighlight.PropertyName };
                    existingSearchHighlight.AddHighlights(searchHighlight.Highlights);

                    SearchHighlights.Add(existingSearchHighlight);
                }
                else
                {
                    existingSearchHighlight.AddHighlights(searchHighlight.Highlights);
                }
            }
        }
    }
}
