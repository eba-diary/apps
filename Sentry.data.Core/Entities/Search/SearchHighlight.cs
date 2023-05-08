using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class SearchHighlight
    {
        public string PropertyName { get; set; }
        public List<string> Highlights { get; set; }

        public void AddHighlights(List<string> highlights)
        {
            if (Highlights == null)
            {
                Highlights = new List<string>();
            }

            Highlights.AddRange(highlights);
        }
    }
}
