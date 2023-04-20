using Sentry.data.Core;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class MigrationHistoryDetailPageModel
    {
        public IList<MigrationHistoryModel> MigrationHistoryModels { get; set; }
        public UserSecurity Security { get; set; }
    }
}