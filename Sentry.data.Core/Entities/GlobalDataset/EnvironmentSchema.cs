using Nest;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class EnvironmentSchema
    {
        [PropertyName("schemaid")]
        public int SchemaId { get; set; }

        [PropertyName("schemaname")]
        [GlobalSearchField(SearchDisplayNames.GlobalDataset.SCHEMANAME)]
        public string SchemaName { get; set; }

        [PropertyName("schemadescription")]
        [GlobalSearchField(SearchDisplayNames.GlobalDataset.SCHEMADESCRIPTION)]
        public string SchemaDescription { get; set; }

        [PropertyName("schemasaidassetcode")]
        [FilterSearchField(FilterCategoryNames.Dataset.PRODUCERASSET, hideResultCounts: true)]
        public string SchemaSaidAssetCode { get; set; }
    }
}