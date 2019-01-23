using System.Collections.Generic;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class BusinessIntelligenceDetailModel : BusinessIntelligenceModel
    {

        public BusinessIntelligenceDetailModel(BusinessIntelligenceDetailDto dto) : base(dto)
        {
            FrequencyDescription = dto.FrequencyDescription;
            TagNames = dto.TagNames;
            CanManageReport = dto.CanManageReport;
        }

        public string FrequencyDescription { get; set; }
        public List<string> TagNames { get; set; }

        public bool CanManageReport { get; set; }
    }
}