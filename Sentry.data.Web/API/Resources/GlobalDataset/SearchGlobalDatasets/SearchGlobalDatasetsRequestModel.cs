namespace Sentry.data.Web.API
{
    public class SearchGlobalDatasetsRequestModel : BaseGlobalDatasetRequestModel
    {
        public bool ShouldSearchColumns { get; set; }
    }
}