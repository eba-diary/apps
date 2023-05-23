using Sentry.data.Web.API;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;

namespace Sentry.data.Web
{
    public class GlobalDatasetViewModel : BaseGlobalDatasetModel
    {
        public string GetSearchHighlightsHtml()
        {
            StringBuilder builder = new StringBuilder();

            foreach (SearchHighlightModel searchHighlight in SearchHighlights)
            {
                builder.Append($"<h4 class='highlightPropertyName p-0 mt-2 text-info'>{searchHighlight.PropertyName}</h4>");
                builder.Append("<ul class='highlightList mb-0 pl-4'>");

                foreach (string highlight in searchHighlight.Highlights.Take(9).ToList())
                {
                    builder.Append($"<li>{highlight}</li>");
                }

                if (searchHighlight.Highlights.Count == 10)
                {
                    builder.Append($"<li>{searchHighlight.Highlights[9]}</li>");
                }
                else if (searchHighlight.Highlights.Count > 10)
                {
                    builder.Append($"<li>...</li>");
                }

                builder.Append("</ul>");
            }

            return builder.ToString();
        }
    }
}