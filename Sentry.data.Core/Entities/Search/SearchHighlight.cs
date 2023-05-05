using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class SearchHighlight
    {
        public string PropertyName { get; set; }
        public List<string> Highlights { get; set; }

        public void AddHighlight(string highlight)
        {
            if (Highlights == null)
            {
                Highlights = new List<string>();
            }

            Highlights.Add(highlight);
        }
    }
}
