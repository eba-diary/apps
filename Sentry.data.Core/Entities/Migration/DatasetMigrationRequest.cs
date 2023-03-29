using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DatasetMigrationRequest : BaseMigrationRequest
    {
        public int SourceDatasetId { get; set; }
        public List<DatasetSchemaMigrationRequest> SchemaMigrationRequests { get; set; } = new List<DatasetSchemaMigrationRequest>();

        public List<string> AllNamedEnvironments { get; set; } = new List<string>();
    }
}
