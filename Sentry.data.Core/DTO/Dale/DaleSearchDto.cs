using Sentry.data.Core.GlobalEnums;

namespace Sentry.data.Core
{
    public class DaleSearchDto
    {
        public string Criteria { get; set; }
        public DaleDestiny Destiny { get; set; }
        public DaleSensitive Sensitive { get; set; }
        public DaleAdvancedCriteriaDto AdvancedCriteria { get; set; }

    }
}
