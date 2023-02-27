namespace Sentry.data.Web.API
{
    public abstract class BaseImmutableSchemaModel : BaseSchemaModel, ISaidEnvironmentModel
    {
        public int DatasetId { get; set; }
        public string SchemaName { get; set; }
        public string SaidAssetCode { get; set; }
        public string NamedEnvironment { get; set; }
        public string NamedEnvironmentTypeCode { get; set; }
    }
}