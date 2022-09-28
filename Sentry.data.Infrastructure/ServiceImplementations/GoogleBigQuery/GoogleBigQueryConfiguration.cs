namespace Sentry.data.Infrastructure
{
    public class GoogleBigQueryConfiguration
    {
        public string BaseUri { get; set; }
        public string RelativeUri { get; set; }
        public string ProjectId { get; set; }
        public string DatasetId { get; set; }
        public string TableId { get; set; }
        public string PageToken { get; set; }
        public int LastIndex { get; set; }
        public int TotalRows { get; set; }
    }
}
