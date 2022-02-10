
using System;
using System.Collections.Generic;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class DatasetDetailModel : DatasetModel
    {

        public DatasetDetailModel(DatasetDetailDto dto) : base(dto)
        {
            Downloads = dto.Downloads;
            DatasetFileCount = dto.DatasetFileCount;
            DatasetFileConfigNames = dto.DatasetFileConfigNames;
            DatasetScopeTypeNames = dto.DatasetScopeTypeNames;
            OriginationCode = dto.OriginationCode;
            DistinctFileExtensions = dto.DistinctFileExtensions;
            DataClassificationDescription = dto.DataClassificationDescription;
            GroupAccessCount = dto.GroupAccessCount;
            
            //foreach (var item in dto.DataFlows)
            //{
            //    DataFlows.Add(new Tuple<string, List<AssociatedDataFlowModel>>(item.Item1, item.Item2.ToModel()));
            //}
        }

        public DatasetDetailModel() { }

        public int Downloads { get; set; }
        public int GroupAccessCount { get; set; }
        public string OriginationCode { get; set; }
        public int DatasetFileCount { get; set; }
        public Dictionary<string, string> DatasetFileConfigNames { get; set; }
        public Dictionary<string, string> DatasetScopeTypeNames { get; set; }
        public List<string> DistinctFileExtensions { get; set; }
        public string DataClassificationDescription { get; set; }
        public List<Tuple<string, List<AssociatedDataFlowModel>>> DataFlows { get; set; }
        public bool DisplayDataflowMetadata { get; set; }
        public bool DisplayTabSections { get; set; }
        public bool DisplaySchemaSearch { get; set; }
    }
}