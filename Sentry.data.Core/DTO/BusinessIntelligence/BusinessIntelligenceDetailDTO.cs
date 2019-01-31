
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class BusinessIntelligenceDetailDto : BusinessIntelligenceDto
    {
        public string FrequencyDescription { get; set; }
        public List<string> TagNames { get; set; }
        public bool CanManageReport { get; set; }
    }
}
