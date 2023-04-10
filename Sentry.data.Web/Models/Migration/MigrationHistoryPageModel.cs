using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public class MigrationHistoryPageModel
    {
        public int? SourceDatasetId { get; set; }
        public string SourceDatasetName { get; set; }
        public bool ShowNamedEnvironmentFilter { get; set; }
        public List<DatasetRelativeModel> DatasetRelatives { get; set; }
    }
}