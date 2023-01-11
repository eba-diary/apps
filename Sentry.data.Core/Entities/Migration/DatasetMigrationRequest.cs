using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Migration
{
    public class DatasetMigrationRequest : MigrationRequest
    {
        public int SourceDatasetId { get; set; }
        public List<SchemaMigrationRequest> SchemaMigrationRequests { get; set; } = new List<SchemaMigrationRequest>();



        public List<string> AllNamedEnvironments { get; set; } = new List<string>();
    }
}
