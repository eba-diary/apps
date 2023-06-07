using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
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
        // converts dataflowmetric into dto to be split into groups based on datasetfileid
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
                /*DataFlowStepName = _context.DataFlowStep.Where(w => w.Id == entity.DataFlowStepId).Select(x => x.Action.Name).FirstOrDefault()*/
                DataFlowStepName = "TEST"
            };
        }

        //Takes search dto with DatasetId, SchemaId, and an optional DatasetFileId, returns elastic result with all matching flow events and divides them into groups based on fileId
        public List<DataFileFlowMetricsDto> GetFileMetricGroups(DataFlowMetricSearchDto searchDto)
        {
            List<DataFlowMetric> entityList = _dataFlowMetricProvider.GetDataFlowMetrics(searchDto);
            List<DataFlowMetricDto> dtoList = entityList.Select(x => MapToDto(x)).ToList();
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

        public long GetAllTotalFilesCount()
        {
            long docCount = _dataFlowMetricProvider.GetAllTotalFiles().SearchTotal;

            return docCount;
        }

        public List<DatasetProcessActivityDto> GetAllTotalFiles()
        {
            DataFlowMetricSearchResultDto dataFlowMetricSearchResultDto = _dataFlowMetricProvider.GetAllTotalFiles().ToDto();

            return MapDatasetProcessActivityDtos(dataFlowMetricSearchResultDto);
        }


        public List<SchemaProcessActivityDto> GetAllTotalFilesByDataset(int datasetId)
        {
            DataFlowMetricSearchResultDto dataFlowMetricSearchResultDto = _dataFlowMetricProvider.GetAllTotalFilesByDataset(datasetId).ToDto();

            return MapSchemaProcessActivityDtos(dataFlowMetricSearchResultDto);
        }

        public List<DatasetFileProcessActivityDto> GetAllTotalFilesBySchema(int schemaId)
        {
            DataFlowMetricSearchResultDto dataFlowMetricSearchResultDto = _dataFlowMetricProvider.GetAllTotalFilesBySchema(schemaId).ToDto();

            return MapDatasetFileProcessActivityDtos(dataFlowMetricSearchResultDto);
        }

        public long GetAllFailedFilesCount()
        {
            long docCount = _dataFlowMetricProvider.GetAllFailedFiles().SearchTotal;

            return docCount;
        }

        public List<DatasetProcessActivityDto> GetAllFailedFiles()
        {
            DataFlowMetricSearchResultDto dataFlowMetricSearchResultDto = _dataFlowMetricProvider.GetAllFailedFiles().ToDto();

            return MapDatasetProcessActivityDtos(dataFlowMetricSearchResultDto);
        }


        public List<SchemaProcessActivityDto> GetAllFailedFilesByDataset(int datasetId)
        {
            DataFlowMetricSearchResultDto dataFlowMetricSearchResultDto = _dataFlowMetricProvider.GetAllFailedFilesByDataset(datasetId).ToDto();

            return MapSchemaProcessActivityDtos(dataFlowMetricSearchResultDto);
        }

        public List<DatasetFileProcessActivityDto> GetAllFailedFilesBySchema(int schemaId)
        {
            DataFlowMetricSearchResultDto dataFlowMetricSearchResultDto = _dataFlowMetricProvider.GetAllFailedFilesBySchema(schemaId).ToDto();

            return MapDatasetFileProcessActivityDtos(dataFlowMetricSearchResultDto);
        }

        #region PRIVATE PROCESS ACTIVITY MAPPING FUNCTIONS
        public List<DatasetProcessActivityDto> MapDatasetProcessActivityDtos(DataFlowMetricSearchResultDto dataFlowMetricSearchResultDto)
        {
            List<DataFlowMetricDto> dataFlowMetricDtoList = dataFlowMetricSearchResultDto.DataFlowMetricResults.Select(x =>
                                                                                                                MapToDto(x)).ToList();

            List<DatasetProcessActivityDto> datasetProcessActivityDtos = new List<DatasetProcessActivityDto>();

            foreach (DataFlowMetricSearchAggregateDto item in dataFlowMetricSearchResultDto.TermAggregates)
            {
                DatasetProcessActivityDto datasetProcessActivityDto = new DatasetProcessActivityDto();

                /*Dataset currentDataset = _context.Datasets.Where(x => x.DatasetId == item.key).FirstOrDefault();*/

                DateTime lastEventTime = dataFlowMetricDtoList.Where(x => x.DatasetId == item.key).Max(x => x.MetricGeneratedDateTime);

                if (lastEventTime != null)
                {
                    /*datasetProcessActivityDto.DatasetName = currentDataset.DatasetName;*/
                    datasetProcessActivityDto.DatasetName = "DATASET";
                    datasetProcessActivityDto.DatasetId = item.key;
                    datasetProcessActivityDto.FileCount = item.docCount;
                    datasetProcessActivityDto.LastEventTime = lastEventTime;

                    datasetProcessActivityDtos.Add(datasetProcessActivityDto);
                }
            }

            return datasetProcessActivityDtos;
        }

        private List<SchemaProcessActivityDto> MapSchemaProcessActivityDtos(DataFlowMetricSearchResultDto dataFlowMetricSearchResultDto)
        {
            List<DataFlowMetricDto> dataFlowMetricDtoList = dataFlowMetricSearchResultDto.DataFlowMetricResults.Select(x =>
                                                                                                                MapToDto(x)).ToList();
            List<SchemaProcessActivityDto> schemaProcessActivityDtos = new List<SchemaProcessActivityDto>();

            foreach (DataFlowMetricSearchAggregateDto item in dataFlowMetricSearchResultDto.TermAggregates)
            {
                SchemaProcessActivityDto schemaProcessActivityDto = new SchemaProcessActivityDto();

                /*Schema currentSchema = _context.Schema.Where(x => x.SchemaId == item.key).FirstOrDefault();*/

                DateTime lastEventTime = dataFlowMetricDtoList.Where(x => x.SchemaId == item.key).Max(x => x.MetricGeneratedDateTime);

                if (lastEventTime != null)
                {
                    /*schemaProcessActivityDto.SchemaName = currentSchema.SchemaEntity_NME;*/
                    schemaProcessActivityDto.SchemaName = "SCHEMA";
                    schemaProcessActivityDto.DatasetId = dataFlowMetricDtoList[0].DatasetId;
                    schemaProcessActivityDto.SchemaId = item.key;
                    schemaProcessActivityDto.FileCount = item.docCount;
                    schemaProcessActivityDto.LastEventTime = lastEventTime;

                    schemaProcessActivityDtos.Add(schemaProcessActivityDto);
                }
            }

            return schemaProcessActivityDtos;
        }

        private List<DatasetFileProcessActivityDto> MapDatasetFileProcessActivityDtos(DataFlowMetricSearchResultDto dataFlowMetricSearchResultDto)
        {
            List<DataFlowMetricDto> dataFlowMetricDtoList = dataFlowMetricSearchResultDto.DataFlowMetricResults.Select(x =>
                                                                                                               MapToDto(x)).ToList();

            List<DatasetFileProcessActivityDto> datasetFileProcessActivityDtos = new List<DatasetFileProcessActivityDto>();

            foreach (DataFlowMetricDto items in dataFlowMetricDtoList)
            {
                DatasetFileProcessActivityDto datasetFileProcessActivityDto = new DatasetFileProcessActivityDto()
                {
                    FileName = items.FileName,
                    FlowExecutionGuid = items.FlowExecutionGuid,
                    LastFlowStep = items.TotalFlowSteps,
                    LastEventTime = items.MetricGeneratedDateTime
                };

                datasetFileProcessActivityDtos.Add(datasetFileProcessActivityDto);
            }

            return datasetFileProcessActivityDtos;
        }
        #endregion
    }
}
