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
        public List<DataFlowMetricEntity> GetDataFlowMetricEntities(DataFlowMetricSearchDto dto)
        {
            return _dataFlowMetricProvider.GetDataFlowMetricEntities(dto);
        }
        public DataFlowMetricDto ToDto(DataFlowMetricEntity entity)
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
        public List<DataFlowMetricDto> GetMetricList(List<DataFlowMetricEntity> entityList)
        {
            List<DataFlowMetricDto> dataFlowMetricDtos = new List<DataFlowMetricDto>();
            foreach(DataFlowMetricEntity entity in entityList)
            {
                DataFlowMetricDto dto = ToDto(entity);
                dataFlowMetricDtos.Add(dto);
            }
            return dataFlowMetricDtos;
        }
        public List<DataFileFlowMetricsDto> GetFileMetricGroups(List<DataFlowMetricDto> dtoList)
        {
            List<DataFileFlowMetricsDto> fileGroups = new List<DataFileFlowMetricsDto>();
            foreach(DataFlowMetricDto dto in dtoList)
            {
                if (fileGroups.Count == 0)
                {
                    DataFileFlowMetricsDto fileGroup = new DataFileFlowMetricsDto();
                    fileGroup.FileName = dto.FileName;
                    fileGroup.DatasetFileId = dto.DatasetFileId;
                    fileGroup.FirstEventTime = dto.MetricGeneratedDateTime;
                    fileGroup.LastEventTime = dto.MetricGeneratedDateTime;
                    fileGroup.Duration = (fileGroup.LastEventTime - fileGroup.FirstEventTime).TotalSeconds.ToString();
                    fileGroup.FlowEvents.Add(dto);
                    fileGroup.TargetCode = "target" + fileGroup.DatasetFileId.ToString();
                    fileGroups.Add(fileGroup);
                }
                else
                {
                    bool present = false;
                    foreach(DataFileFlowMetricsDto fileGroup in fileGroups)
                    {
                        if (dto.FileName == fileGroup.FileName)
                        {
                            present = true;
                            if(fileGroup.FirstEventTime > dto.MetricGeneratedDateTime)
                            {
                                fileGroup.FirstEventTime = dto.MetricGeneratedDateTime;
                            }
                            if(fileGroup.LastEventTime < dto.MetricGeneratedDateTime)
                            {
                                fileGroup.LastEventTime = dto.MetricGeneratedDateTime;
                            }
                            fileGroup.Duration = (fileGroup.LastEventTime - fileGroup.FirstEventTime).TotalSeconds.ToString();
                            fileGroup.FlowEvents.Add(dto);
                        }
                    }
                    if (present == false)
                    {
                        DataFileFlowMetricsDto fileGroup = new DataFileFlowMetricsDto();
                        fileGroup.FileName = dto.FileName;
                        fileGroup.DatasetFileId = dto.DatasetFileId;
                        fileGroup.FirstEventTime = dto.MetricGeneratedDateTime;
                        fileGroup.LastEventTime = dto.MetricGeneratedDateTime;
                        fileGroup.Duration = (fileGroup.LastEventTime - fileGroup.FirstEventTime).TotalSeconds.ToString();
                        fileGroup.FlowEvents.Add(dto);
                        fileGroup.TargetCode = "target" + fileGroup.DatasetFileId.ToString();
                        fileGroups.Add(fileGroup);
                    }
                }
            }
            return fileGroups;
        }
        public List<DataFileFlowMetricsDto> SortFlowMetrics(List<DataFileFlowMetricsDto> dtoList)
        {
            foreach(DataFileFlowMetricsDto dto in dtoList)
            {
                dto.FlowEvents.Sort();
            }
            dtoList.Sort();
            return dtoList;
        }
        public void GetFileFlowMetricsStatus(List<DataFileFlowMetricsDto> SortedDtoList)
        {
            foreach(DataFileFlowMetricsDto dto in SortedDtoList)
            {
                if (dto.FlowEvents[0].StatusCode == "C")
                {
                    dto.AllEventsComplete = true;
                }
                if (dto.FlowEvents[0].CurrentFlowStep == dto.FlowEvents[0].TotalFlowSteps)
                {
                    dto.AllEventsPresent = true;
                }
            }
        }
    }
}
