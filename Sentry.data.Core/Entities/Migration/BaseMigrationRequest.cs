using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class BaseMigrationRequest
    {
        public int TargetDatasetId { get; set; }
        public string TargetDatasetNamedEnvironment { get; set; }
        public NamedEnvironmentType TargetDatasetNamedEnvironmentType { get; set; }
    }
}
