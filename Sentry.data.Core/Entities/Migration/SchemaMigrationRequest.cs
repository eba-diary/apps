using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Migration
{
    public class SchemaMigrationRequest : MigrationRequest
    {
        public int SourceSchemaId { get; set; }
        public string TargetDataFlowNamedEnvironment { get; set; }
    }
}
