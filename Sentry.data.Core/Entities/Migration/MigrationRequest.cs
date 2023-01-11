using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class MigrationRequest
    {
        public int TargetDatasetId { get; set; }
        public string TargetDatasetNamedEnvironment { get; set; }
    }
}
