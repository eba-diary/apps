using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class BusinessIntelligenceTileDto : DatasetTileDto
    {
        [FilterSearchField(FilterCategoryNames.BusinessIntelligence.REPORTTYPE, hideResultCounts: true)]
        public string ReportType { get; set; }
        public string UpdateFrequency { get; set; }
        public string ContactNames { get; set; }
        [FilterSearchField(FilterCategoryNames.BusinessIntelligence.BUSINESSUNIT, hideResultCounts: true)]
        public List<string> BusinessUnits { get; set; } = new List<string>();
        [FilterSearchField(FilterCategoryNames.BusinessIntelligence.FUNCTION, hideResultCounts: true)]
        public List<string> Functions { get; set; } = new List<string>();
        [FilterSearchField(FilterCategoryNames.BusinessIntelligence.TAG, hideResultCounts: true)]
        public List<string> Tags { get; set; } = new List<string>();
    }
}
