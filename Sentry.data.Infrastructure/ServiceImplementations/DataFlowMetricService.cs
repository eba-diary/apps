using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;

namespace Sentry.data.Infrastructure
{
    public class DataFlowMetricService: IDataFlowMetricService
    {
        private readonly IDataFlowMetricProvider _dataFlowMetricProvider;
        private readonly IDatasetContext _context;
        public DataFlowMetricService(IDataFlowMetricProvider dataFlowMetricProvider, IDatasetContext context)
        {
            _dataFlowMetricProvider = dataFlowMetricProvider;
            _context = context;
        }
        private DataFlowMetricDto MapToDto(DataFlowMetric entity)
        {
            return new DataFlowMetricDto()
            {
                QueryMadeDateTime = entity.QueryMadeDateTime,
                SchemaId = entity.SchemaId,
                EventContents = entity.EventContents,
                TotalFlowSteps = entity.MaxExecutionOrder,
                FileModifiedDateTime = entity.FileModifiedDateTime,
                OriginalFileName = entity.OriginalFileName,
                DatasetId = entity.DatasetId,
                CurrentFlowStep = entity.ExecutionOrder,
                DataActionId = entity.DataActionId,
                DataFlowId = entity.DataFlowId,
                Partition = entity.Partition,
                DataActionTypeId = entity.DataActionTypeId,
                MessageKey = entity.MessageKey,
                Duration = entity.Duration,
                Offset = entity.Offset,
                DataFlowName = entity.DataFlowName,
                DataFlowStepId = entity.DataFlowStepId,
                FlowExecutionGuid = entity.FlowExecutionGuid,
                FileSize = entity.FileSize,
                EventMetricId = entity.EventMetricId,
                StorageCode = entity.StorageCode,
                FileCreatedDateTime = entity.FileCreatedDateTime,
                RunInstanceGuid = entity.RunInstanceGuid,
                FileName = entity.FileName,
                SaidKeyCode = entity.SaidKeyCode,
                MetricGeneratedDateTime = entity.EventMetricCreatedDateTime,
                DatasetFileId = entity.DatasetFileId,
                ProcessStartDateTime = entity.ProcessStartDateTime,
                StatusCode = entity.StatusCode,
                DataFlowStepName = _context.DataFlowStep.Where(w => w.Id == entity.DataFlowStepId).Select(x => x.Action.Name).FirstOrDefault()
            };
        }
        private List<DataFlowMetricDto> GetMetricList(List<DataFlowMetric> entityList)
        {
            List<DataFlowMetricDto> dataFlowMetricDtos = entityList.Select(x => MapToDto(x)).ToList();
            return dataFlowMetricDtos;
        }
        public List<DataFileFlowMetricsDto> GetFileMetricGroups(DataFlowMetricSearchDto searchDto)
        {
            List<DataFlowMetric> entityList = _dataFlowMetricProvider.GetDataFlowMetrics(searchDto);
            List<DataFlowMetricDto> dtoList = GetMetricList(entityList);
            List<DataFileFlowMetricsDto> fileGroups = new List<DataFileFlowMetricsDto>();
            var grouped = dtoList.GroupBy(x => x.FileName).ToList();
            foreach(var group in grouped)
            {
                DataFileFlowMetricsDto fileGroup = new DataFileFlowMetricsDto()
                {
                    FileName = group.Key,
                    DatasetFileId = group.First().DatasetFileId,
                    FlowEvents = group.OrderByDescending(x => x.EventMetricId).ToList(),
                };
                DataFlowMetricDto mostRecentMetric = fileGroup.FlowEvents.First();
                fileGroup.FirstEventTime = group.Where(x => x.RunInstanceGuid == mostRecentMetric.RunInstanceGuid).Min(x => x.MetricGeneratedDateTime);
                fileGroup.LastEventTime = group.Where(x => x.RunInstanceGuid == mostRecentMetric.RunInstanceGuid).Max(x => x.MetricGeneratedDateTime);
                fileGroup.Duration = (fileGroup.LastEventTime - fileGroup.FirstEventTime).TotalSeconds.ToString();
                fileGroup.TargetCode = "target" + fileGroup.DatasetFileId.ToString();
                fileGroup.AllEventsPresent = mostRecentMetric.TotalFlowSteps == mostRecentMetric.CurrentFlowStep;
                fileGroup.AllEventsComplete = mostRecentMetric.StatusCode == "C";
                fileGroups.Add(fileGroup);
            }
            fileGroups = fileGroups.OrderByDescending(x => x.FirstEventTime).ToList();

            return fileGroups;
        }
    }
}
