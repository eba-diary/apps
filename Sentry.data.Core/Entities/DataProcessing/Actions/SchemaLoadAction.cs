using System.Collections.Generic;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class SchemaLoadAction : BaseAction
    {
        public SchemaLoadAction()
        {
            TargetStoragePrefix = "temp-file/schemaload/";
        }
        public virtual List<JobSchemaMap> SchemaMaps { get; set; }
    }
}
