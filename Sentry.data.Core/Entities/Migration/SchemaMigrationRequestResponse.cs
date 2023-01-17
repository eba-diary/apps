using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SchemaMigrationRequestResponse
    {
        public bool MigratedSchema { get; set; }
        public int TargetSchemaId { get; set; }
        public string SchemaMigrationReason { get; set; }

        public bool MigratedSchemaRevision { get; set; }
        public int TargetSchemaRevisionId { get; set; }
        public string SchemaRevisionMigrationReason { get; set; }

        public bool MigratedDataFlow { get; set; }
        public int TargetDataFlowId { get; set; }
        public string DataFlowMigrationReason { get; set; }
    }
}
