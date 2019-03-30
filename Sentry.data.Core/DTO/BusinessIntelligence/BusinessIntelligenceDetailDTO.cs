
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class BusinessIntelligenceDetailDto : BusinessIntelligenceDto
    {
        public string FrequencyDescription { get; set; }
        public List<string> TagNames { get; set; }
        public List<string> FunctionNames { get; set; }
        public List<string> BusinessUnitNames { get; set; }
        public List<string> Images { get; set; }
        public List<int> ImageIds { get; set; }
    }
}
