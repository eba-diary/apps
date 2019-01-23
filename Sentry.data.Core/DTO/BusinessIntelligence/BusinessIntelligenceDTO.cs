using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class BusinessIntelligenceDto : BaseEntityDto
    {

        public string Location { get; set; }
        public string LocationType { get; set; }
        public int FileTypeId { get; set; }
        public int FrequencyId { get; set; }

    }
}
