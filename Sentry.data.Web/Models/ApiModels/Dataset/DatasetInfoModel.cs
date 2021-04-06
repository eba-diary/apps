namespace Sentry.data.Web.Models.ApiModels.Dataset
{
    public class DatasetInfoModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSecure { get; set; }
        public string PrimaryContactName { get; set; }
        public string PrimarContactEmail { get; set; }
        public string PrimaryOwnerName { get; set; }
        public string Category { get; set; }
        public string ObjectStatus { get; set; }
    }
}