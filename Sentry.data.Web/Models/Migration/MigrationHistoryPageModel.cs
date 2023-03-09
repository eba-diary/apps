using Sentry.data.Core;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class MigrationHistoryPageModel
    {
        public int? SourceDatasetId { get; set; }
        public string SourceDatasetName { get; set; }
        public IList<MigrationHistoryModel> MigrationHistoryModels { get; set; }
        public UserSecurity Security { get; set; }
    }
}