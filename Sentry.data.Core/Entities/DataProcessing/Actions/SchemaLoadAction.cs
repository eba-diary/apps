using Sentry.data.Core.Interfaces.DataProcessing;
using System.Collections.Generic;

namespace Sentry.data.Core.Entities.DataProcessing
{
    public class SchemaLoadAction : BaseAction
    {
        public SchemaLoadAction() { }
        public SchemaLoadAction(ISchemaLoadProvider schemaLoadProvider) : base(schemaLoadProvider)
        {
            TargetStoragePrefix = GlobalConstants.DataFlowTargetPrefixes.SCHEMA_LOAD_PREFIX + Configuration.Config.GetHostSetting("S3DataPrefix");
        }
        public virtual List<SchemaMap> SchemaMaps { get; set; }
    }
}
