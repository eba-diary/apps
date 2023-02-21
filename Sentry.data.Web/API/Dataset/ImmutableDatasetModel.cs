namespace Sentry.data.Web.API
{
    public abstract class ImmutableDatasetModel : BaseDatasetModel
    {
        public string DatasetName { get; set; }
        public string CategoryName { get; set; }
        public string ShortName { get; set; }
        public string SaidAssetCode { get; set; }
        public string NamedEnvironment { get; set; }
        public string NamedEnvironmentTypeCode { get; set; }
    }
}