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
        public static List<DataFlowMetricGroupModel> ToModels(List<DataFileFlowMetricsDto> fileGroups)
        {
            List<DataFlowMetricGroupModel> models = new List<DataFlowMetricGroupModel>();
            foreach(DataFileFlowMetricsDto fileGroup in fileGroups)
            {
                models.Add(new DataFlowMetricGroupModel()
                {
                    DatasetFileId = fileGroup.DatasetFileId,
                    FileName = fileGroup.FileName,
                    FirstEventTime = fileGroup.FirstEventTime,
                    LastEventTime = fileGroup.LastEventTime,
                    Duration = fileGroup.Duration,
                    FlowEvents = fileGroup.FlowEvents,
                    AllEventsPresent = fileGroup.AllEventsPresent,
                    AllEventsComplete = fileGroup.AllEventsComplete,
                    TargetCode = fileGroup.TargetCode,
                });
            }
            return models;
        }
    }
}