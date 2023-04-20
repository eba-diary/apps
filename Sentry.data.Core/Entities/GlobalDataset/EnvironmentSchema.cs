using Nest;

namespace Sentry.data.Core
{
    public class EnvironmentSchema
    {
        [PropertyName("schemaid")]
        public int SchemaId { get; set; }
        [PropertyName("schemaname")]
        public string SchemaName { get; set; }
        [PropertyName("schemadescription")]
        public string SchemaDescription { get; set; }
        [PropertyName("schemasaidassetcode")]
        public string SchemaSaidAssetCode { get; set; }
    }
}