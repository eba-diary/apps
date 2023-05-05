namespace Sentry.data.Core
{
    public class SearchGlobalDatasetsDto : HighlightableFilterSearchDto
    {
        public bool ShouldSearchColumns { get; set; }
    }
}
