using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class BusinessIntelligenceTileDto : DatasetTileDto
    {
        public List<string> ReportTypes { get; set; } = new List<string>();
        public string UpdateFrequency { get; set; }
        public string ContactNames { get; set; }
        public string AdditionalContactNames { get; set; }
        public List<string> BusinessUnits { get; set; } = new List<string>();
        public List<string> Functions { get; set; } = new List<string>();
        public List<string> Measures { get; set; } = new List<string>();
        public List<string> Attributes { get; set; } = new List<string>();
        public List<string> Others { get; set; } = new List<string>();
    }
}
