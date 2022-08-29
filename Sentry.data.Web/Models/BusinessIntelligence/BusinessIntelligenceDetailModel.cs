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
            FunctionNames = dto.FunctionNames;
            BusinessUnitNames = dto.BusinessUnitNames;
            LocationType = dto.LocationType;
        }

        public string FrequencyDescription { get; set; }
        public List<string> TagNames { get; set; }
        public List<string> FunctionNames { get; set; }
        public List<string> BusinessUnitNames { get; set; }
        public string LocationType { get; set; }
        public bool UseUpdatedSearchPage { get; set; }
    }
}