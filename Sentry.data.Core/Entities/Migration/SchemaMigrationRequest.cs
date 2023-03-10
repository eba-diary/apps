using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SchemaMigrationRequest : BaseMigrationRequest
    {
        public int SourceSchemaId { get; set; }
        public string TargetDataFlowNamedEnvironment { get; set; }

        public static class ValidationErrors
        {
            public const string SourceSchemaIdRequired = "SourceSchemaId is required";
            public const string TargetDataFlowNamedEnvironmentRequired = "TargetDataFlowNamedEnvironment is required";
            public const string TargetDataFlowNamedEnvironmentIsInvalid = "TargetDataFlowNamedEnvironment is invalid";
            public const string DatasetsAreNotRelated = "Target datasets are not related";
        }
    }    
}
