using Sentry.data.Core.GlobalEnums;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DaleSearchDto
    {
        public string Criteria { get; set; }
        public DaleDestiny Destiny { get; set; }
        public DaleSensitive Sensitive { get; set; }
        public DaleAdvancedCriteriaDto AdvancedCriteria { get; set; }
        public string EnvironmentFilter { get; set; } = EnvironmentFilters.PROD;
    }
}
