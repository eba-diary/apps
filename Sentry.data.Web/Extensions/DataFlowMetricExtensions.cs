using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public static class DataFlowMetricExtensions
    {
        public static DataFlowMetricSearchDto ToDto(DataFlowMetricSearchModel model)
        {
            DataFlowMetricSearchDto dto = new DataFlowMetricSearchDto()
            {
                DatasetId = model.DatasetId,
                SchemaId = model.SchemaId,
                DatasetFileId = model.DatasetFileId,
            };
            return dto;
        }
        public static DataFlowMetricGroupModel ToModel(List<DataFileFlowMetricsDto> fileGroups)
        {
            DataFlowMetricGroupModel model = new DataFlowMetricGroupModel()
            {
                DataFlowMetricGroups = fileGroups,
            };
            return model;
        }
    }
}