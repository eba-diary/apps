namespace Sentry.data.Web.API
{
    public class IndexGlobalDatasetsResponseModel : IResponseModel
    {
        public string BackgroundJobId { get; set; }
        public int IndexCount { get; set; }
        public int DeleteCount { get; set; }
    }
}