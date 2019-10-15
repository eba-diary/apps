using System.Collections.Generic;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class SchemaLoadAction : BaseAction
    {
        public SchemaLoadAction()
        {
            TargetStoragePrefix = GlobalConstants.DataFlowTargetPrefixes.SCHEMA_LOAD_PREFIX;
        }
        public virtual List<JobSchemaMap> SchemaMaps { get; set; }
    }
}
