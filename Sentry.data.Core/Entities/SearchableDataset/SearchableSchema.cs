using Nest;

namespace Sentry.data.Core
{
    public class SearchableSchema
    {
        [PropertyName("schemaid")]
        public int SchemaId { get; set; }
        [PropertyName("schemaname")]
        public string SchemaName { get; set; }
        [PropertyName("schemadescription")]
        public string SchemaDescription { get; set; }
        [PropertyName("producerasset")]
        public string ProducerAsset { get; set; }
    }
}