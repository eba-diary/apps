
using System;
using System.Collections.Generic;
using System.Linq;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class DatasetDetailModel : DatasetModel
    {
        public DatasetDetailModel(DatasetDetailDto dto) : base(dto)
        {
            Downloads = dto.Downloads;
            DatasetFileCount = dto.DatasetFileCount;
            DatasetFileConfigSchemas = dto.DatasetFileConfigSchemas?.Select(x => x.ToModel()).ToList();
            DatasetScopeTypeNames = dto.DatasetScopeTypeNames;
            OriginationCode = dto.OriginationCode;
            DataClassificationDescription = dto.DataClassificationDescription;
            GroupAccessCount = dto.GroupAccessCount;
            HasDataAccess = dto.Security.CanViewData;
            HasSchema = dto.DatasetFileConfigSchemas.Any();
            DatasetRelatives = dto.DatasetRelatives?.Select(s => s.ToModel()).ToList();
        }

        public DatasetDetailModel() { }

        public int Downloads { get; set; }
        public int GroupAccessCount { get; set; }
        public string OriginationCode { get; set; }
        public int DatasetFileCount { get; set; }
        public List<DatasetFileConfigSchemaModel> DatasetFileConfigSchemas { get; set; }
        public Dictionary<string, string> DatasetScopeTypeNames { get; set; }
        public string DataClassificationDescription { get; set; }
        public List<DatasetRelativeModel> DatasetRelatives { get; set; }



        //NOTE: USED BY KNOCKOUT AS MODEL TO DISPLAY ELEMENTS
        public List<Tuple<string, List<AssociatedDataFlowModel>>> DataFlows { get; set; }
        public bool DisplayDataflowMetadata { get; set; }
        public bool DisplaySchemaSearch { get; set; }
        public bool DisplayDataflowEdit { get; set; }
        public bool DisplayDatasetFileDelete { get; set; }
        public bool DisplayDatasetFileUpload { get; set; }
        public bool HasDataAccess { get; set; }
        public bool ShowManagePermissionsLink { get; set; }
        public bool UseUpdatedSearchPage { get; set; }
        public bool HasSchema { get; set; }
        public bool CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL { get; set; }
    }
}