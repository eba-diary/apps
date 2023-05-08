using Nest;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class EnvironmentSchema
    {
        [PropertyName("schemaid")]
        public int SchemaId { get; set; }

        [PropertyName("schemaname")]
        [GlobalSearchField]
        public string SchemaName { get; set; }

        [PropertyName("schemadescription")]
        [GlobalSearchField]
        public string SchemaDescription { get; set; }

        [PropertyName("schemasaidassetcode")]
        [FilterSearchField(FilterCategoryNames.Dataset.PRODUCERASSET, hideResultCounts: true)]
        public string SchemaSaidAssetCode { get; set; }
    }
}