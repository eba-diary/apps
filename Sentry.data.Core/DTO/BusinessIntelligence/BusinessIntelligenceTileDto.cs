using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class BusinessIntelligenceTileDto : DatasetTileDto
    {
        public string AbbreviatedCategories { get; set; }
        public string ReportType { get; set; }
        public string UpdateFrequency { get; set; }
        public string ContactNames { get; set; }
        public List<string> BusinessUnits { get; set; } = new List<string>();
        public List<string> Functions { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
    }
}
