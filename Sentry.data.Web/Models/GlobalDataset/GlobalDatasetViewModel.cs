using Sentry.data.Web.API;
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
                foreach (string highlight in searchHighlight.Highlights)
                {
                    builder.Append($"<li>{highlight}</li>");
                }
                builder.Append("</ul>");
            }

            return builder.ToString();
        }
    }
}