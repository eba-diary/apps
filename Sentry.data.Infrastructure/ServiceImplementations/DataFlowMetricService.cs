using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sentry.data.Core;
using Sentry.data.Core.Interfaces;

namespace Sentry.data.Infrastructure
{
    public class DataFlowMetricService: IDataFlowMetricService
    {
        private readonly IDataFlowMetricProvider _dataFlowMetricProvider;
        public DataFlowMetricService(IDataFlowMetricProvider dataFlowMetricProvider)
        {
            _dataFlowMetricProvider = dataFlowMetricProvider;
        }
        public List<DataFlowMetricEntity> GetDataFlowMetricEntities(DataFlowMetricSearchDto dto)
        {
            return _dataFlowMetricProvider.GetDataFlowMetricEntities(dto);
        }
        public List<DataFlowMetricDto> GetMetricList(List<DataFlowMetricEntity> entityList)
        {
            List<DataFlowMetricDto> dataFlowMetricDtos = new List<DataFlowMetricDto>();
            foreach(DataFlowMetricEntity entity in entityList)
            {
                DataFlowMetricDto dto = entity.ToDto();
                dataFlowMetricDtos.Add(dto);
            }
            return dataFlowMetricDtos;
        }
        public List<DataFileFlowMetricsDto> GetFileMetricGroups(List<DataFlowMetricDto> dtoList)
        {
            int totalFileGroups = 0;
            List<DataFileFlowMetricsDto> fileGroups = new List<DataFileFlowMetricsDto>();
            foreach(DataFlowMetricDto dto in dtoList)
            {
                if (fileGroups.Count == 0)
                {
                    DataFileFlowMetricsDto fileGroup = new DataFileFlowMetricsDto();
                    fileGroup.FileName = dto.FileName;
                    fileGroup.FirstEventTime = dto.MetricGeneratedDateTime;
                    fileGroup.LastEventTime = dto.MetricGeneratedDateTime;
                    fileGroup.Duration = fileGroup.LastEventTime - fileGroup.FirstEventTime;
                    fileGroup.FlowEvents.Add(dto);
                    totalFileGroups++;
                    fileGroup.CollapseTarget = "#" + totalFileGroups.ToString();
                    fileGroup.CollapseId = totalFileGroups.ToString();
                    if (dto.TotalFlowteps == dto.CurrentFlowStep)
                    {
                        fileGroup.AllEventsPresent = true;
                    }
                    if (dto.StatusCode != "C")
                    {
                        fileGroup.AllEventsComplete = false;
                    }
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
                            fileGroup.Duration = fileGroup.LastEventTime - fileGroup.FirstEventTime;
                            if(dto.TotalFlowteps == dto.CurrentFlowStep)
                            {
                                fileGroup.AllEventsPresent = true;
                            }
                            if(dto.StatusCode != "C")
                            {
                                fileGroup.AllEventsComplete = false;
                            }
                            fileGroup.FlowEvents.Add(dto);
                        }
                    }
                    if (present == false)
                    {
                        DataFileFlowMetricsDto fileGroup = new DataFileFlowMetricsDto();
                        fileGroup.FileName = dto.FileName;
                        fileGroup.FirstEventTime = dto.MetricGeneratedDateTime;
                        fileGroup.LastEventTime = dto.MetricGeneratedDateTime;
                        fileGroup.Duration = fileGroup.LastEventTime - fileGroup.FirstEventTime;
                        fileGroup.FlowEvents.Add(dto);
                        totalFileGroups++;
                        fileGroup.CollapseTarget = "#" + totalFileGroups.ToString();
                        fileGroup.CollapseId = totalFileGroups.ToString();
                        if (dto.TotalFlowteps == dto.CurrentFlowStep)
                        {
                            fileGroup.AllEventsPresent = true;
                        }
                        if (dto.StatusCode != "C")
                        {
                            fileGroup.AllEventsComplete = false;
                        }
                        fileGroups.Add(fileGroup);
                    }
                }
            }
            return fileGroups;
        }
    }
}
