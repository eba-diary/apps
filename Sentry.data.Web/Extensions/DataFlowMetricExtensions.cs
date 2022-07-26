using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public static class DataFlowMetricExtensions
    {
        public static DataFlowMetricSearchDto ToDto(this DataFlowMetricSearchModel model)
        {
            DataFlowMetricSearchDto dto = new DataFlowMetricSearchDto()
            {
                DatasetId = model.DatasetId,
                SchemaId = model.SchemaId,
                DatasetFileId = model.DatasetFileId,
            };
            return dto;
        }
        public static List<DataFlowMetricGroupModel> ToModels(this List<DataFileFlowMetricsDto> fileGroups)
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
                    FlowEvents = fileGroup.FlowEvents.ToModels(),
                    AllEventsPresent = fileGroup.AllEventsPresent,
                    AllEventsComplete = fileGroup.AllEventsComplete,
                    TargetCode = fileGroup.TargetCode,
                });
            }
            return models;
        }
        public static List<DataFlowMetricModel> ToModels(this List<DataFlowMetricDto> dtoList)
        {
            List<DataFlowMetricModel> models = new List<DataFlowMetricModel>();
            foreach(DataFlowMetricDto dto in dtoList)
            {
                models.Add(new DataFlowMetricModel()
                {
                    QueryMadeDateTime = dto.QueryMadeDateTime,
                    SchemaId = dto.SchemaId,
                    EventContents = dto.EventContents,
                    TotalFlowSteps = dto.TotalFlowSteps,
                    FileModifiedDateTime = dto.FileModifiedDateTime,
                    OriginalFileName = dto.OriginalFileName,
                    DatasetId = dto.DatasetId,
                    CurrentFlowStep = dto.CurrentFlowStep,
                    DataActionId = dto.DataActionId,
                    DataFlowId = dto.DataFlowId,
                    Partition = dto.Partition,
                    DataActionTypeId = dto.DataActionTypeId,
                    MessageKey = dto.MessageKey,
                    Duration = dto.Duration,
                    Offset = dto.Offset,
                    DataFlowName = dto.DataFlowName,
                    DataFlowStepId = dto.DataFlowStepId,
                    DataFlowStepName = dto.DataFlowStepName,
                    FlowExecutionGuid = dto.FlowExecutionGuid,
                    FileSize = dto.FileSize,
                    EventMetricId = dto.EventMetricId,
                    StorageCode = dto.StorageCode,
                    FileCreatedDateTime = dto.FileCreatedDateTime,
                    RunInstanceGuid = dto.RunInstanceGuid,
                    FileName = dto.FileName,
                    SaidKeyCode = dto.SaidKeyCode,
                    MetricGeneratedDateTime = dto.MetricGeneratedDateTime,
                    DatasetFileId = dto.DatasetFileId,
                    ProcessStartDateTime = dto.ProcessStartDateTime,
                    StatusCode = dto.StatusCode,
                });
            }
            return models;
        }
    }
}