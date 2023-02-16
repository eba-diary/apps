namespace Sentry.data.Web.API
{
    public abstract class BaseDatasetModel
    {
        public string DatasetDescription { get; set; }
        public string UsageInformation { get; set; }
        public string DataClassificationTypeCode { get; set; }
        public bool IsSecured { get; set; }
        public string PrimaryContactId { get; set; }
        public string AlternateContactEmail { get; set; }
        public string OriginationCode { get; set; }
        public string OriginalCreator { get; set; }
    }
}