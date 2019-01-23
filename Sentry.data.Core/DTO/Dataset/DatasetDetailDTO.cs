
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DatasetDetailDto : DatasetDto
    {
        public bool CanDwnldSenstive { get; set; }
        public bool CanManageConfigs { get; set; }
        public bool CanDwnldNonSensitive { get; set; }
        public bool CanQueryTool { get; set; }
        public bool CanEditDataset { get; set; }
        public bool CanUpload { get; set; }
        public int Downloads { get; set; }
        public Dictionary<string,string> DatasetFileConfigNames { get; set; }
        public Dictionary<string,string> DatasetScopeTypeNames { get; set; }
        public List<string> DistinctFileExtensions { get; set; }
        public int DatasetFileCount { get; set; }
        public string OriginationCode { get; set; }
        public string DataClassificationDescription { get; set; }
    }
}
