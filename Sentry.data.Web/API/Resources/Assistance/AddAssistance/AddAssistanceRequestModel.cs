namespace Sentry.data.Web.API
{
    public class AddAssistanceRequestModel : IRequestModel
    {
        public string Summary { get; set; }
        public string Description { get; set; }
        public string CurrentPage { get; set; }
        public string ReporterAssociateId { get; set; }
        public string DatasetName { get; set; }
        public string SchemaName { get; set; }
    }
}