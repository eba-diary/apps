
using System.Collections.Generic;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class DatasetDetailModel : DatasetModel
    {

        public DatasetDetailModel(DatasetDetailDto dto) : base(dto)
        {
            CanDwnldSenstive = dto.CanDwnldSenstive;
            CanEditDataset = dto.CanEditDataset;
            CanDwnldNonSensitive = dto.CanDwnldNonSensitive;
            CanQueryTool = dto.CanQueryTool;
            CanUpload = dto.CanUpload;
            Downloads = dto.Downloads;
            DatasetFileCount = dto.DatasetFileCount;
            DatasetFileConfigNames = dto.DatasetFileConfigNames;
            DatasetScopeTypeNames = dto.DatasetScopeTypeNames;
            OriginationCode = dto.OriginationCode;
            DistinctFileExtensions = dto.DistinctFileExtensions;
            DataClassificationDescription = dto.DataClassificationDescription;
        }

        public bool CanDwnldSenstive { get; set; }
        public bool CanEditDataset { get; set; }
        public bool CanDwnldNonSensitive { get; set; }
        public bool CanQueryTool { get; set; }
        public bool CanUpload { get; set; }
        public int Downloads { get; set; }
        public string OriginationCode { get; set; }
        public int DatasetFileCount { get; set; }
        public Dictionary<string, string> DatasetFileConfigNames { get; set; }
        public Dictionary<string, string> DatasetScopeTypeNames { get; set; }
        public List<string> DistinctFileExtensions { get; set; }

        public string DataClassificationDescription { get; set; }
    }
}