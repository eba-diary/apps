using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DatasetMigrationRequestResponse
    {
        public bool IsDatasetMigrated { get; set; }
        public string DatasetMigrationReason { get; set; }
        public int DatasetId { get; set; }
        public List<SchemaMigrationRequestResponse> SchemaMigrationResponses { get; set; } = new List<SchemaMigrationRequestResponse>();
    }
}
