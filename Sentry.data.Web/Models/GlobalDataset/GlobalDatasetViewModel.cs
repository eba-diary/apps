using Sentry.data.Web.API;
using System.Text;

namespace Sentry.data.Web
{
    public class GlobalDatasetViewModel : BaseGlobalDatasetModel
    {
        public string GetSearchHighlightsTooltip()
        {
            StringBuilder builder = new StringBuilder();

            foreach (SearchHighlightModel searchHighlight in SearchHighlights)
            {
                builder.AppendLine($"<div class='col'>{searchHighlight.PropertyName}:</div>");
                foreach (string highlight in searchHighlight.Highlights)
                { 
                    builder.AppendLine(highlight);
                }
            }

            return builder.ToString();
        }
    }
}