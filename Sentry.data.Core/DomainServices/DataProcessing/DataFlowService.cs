using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Interfaces.DataProcessing;


namespace Sentry.data.Core
{
    public class DataFlowService : IDataFlowService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IMessagePublisher _messagePublisher;
        private readonly UserService _userService;
        private readonly IJobService _jobService;


        public DataFlowService(IDatasetContext datasetContext, IMessagePublisher messagePublisher, 
            UserService userService, IJobService jobService)
        {
            _datasetContext = datasetContext;
            _messagePublisher = messagePublisher;
            _userService = userService;
            _jobService = jobService;
        }

        public List<DataFlowDto> ListDataFlows()
        {
            List<DataFlow> dfList = _datasetContext.DataFlow.ToList();
            List<DataFlowDto> dtoList = new List<DataFlowDto>();
            MapToDtoList(dfList, dtoList);
            return dtoList;
        }

        public DataFlowDetailDto GetDataFlowDetailDto(int id)
        {
            DataFlow df = _datasetContext.GetById<DataFlow>(id);
            DataFlowDetailDto dto = new DataFlowDetailDto();
            MapToDetailDto(df, dto);
            return dto;
        }

        public List<DataFlowStepDto> GetDataFlowStepDtoByTrigger(string key)
        {
            List<DataFlowStep> dfsList = _datasetContext.DataFlowStep.Where(w => w.TriggerKey == key).ToList();
            List<DataFlowStepDto> dtoList = new List<DataFlowStepDto>();
            MapToDtoList(dfsList, dtoList);
            return dtoList;
        }

        public bool CreateandSaveDataFlow(DataFlowDto dto)
        {
            try
            {
                CreateDataFlow(dto);

                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("dataflowservice-createandsavedataflow failed to save dataflow", ex);
                return false;
            }

            return true;
        }

        public bool CreateDataFlow(int schemaId)
        {
            int cnt = _datasetContext.DataFlow.Count();
            DataFlow df = new DataFlow()
            {
                Name = "CreateDataFlowTest_" + cnt.ToString(),
                CreatedBy = "072984",
                CreatedDTM = DateTime.Now,
            };

            _datasetContext.Add(df);

            DataFlowStep step1 = new DataFlowStep()
            {
                DataFlow = df,
                Action = _datasetContext.S3DropAction.FirstOrDefault(),
                DataAction_Type_Id = DataActionType.S3Drop
            };

            AddDataFlowStep(df, step1);
            _datasetContext.Add(step1);

            DataFlowStep step3 = new DataFlowStep()
            {
                DataFlow = df,
                Action = _datasetContext.RawStorageAction.FirstOrDefault(),
                DataAction_Type_Id = DataActionType.RawStorage
            };

            AddDataFlowStep(df, step3);
            _datasetContext.Add(step3);

            DataFlowStep step2 = new DataFlowStep()
            {
                DataFlow = df,
                Action = _datasetContext.SchemaLoadAction.FirstOrDefault(),
                DataAction_Type_Id = DataActionType.SchemaLoad
            };

            AddDataFlowStep(df, step2);
            _datasetContext.Add(step2);

            SchemaMap mapping = new SchemaMap()
            {
                DataFlowStepId = step2,
                MappedSchema = _datasetContext.FileSchema.Where(w => w.SchemaId == schemaId).FirstOrDefault(),
                Dataset = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schemaId).Select(s => s.ParentDataset).FirstOrDefault(),
                SearchCriteria = "Testfile.csv"
            };
            _datasetContext.Add(mapping);
            List<SchemaMap> maps = new List<SchemaMap>();
            maps.Add(mapping);
            step2.SchemaMappings = maps;

            DataFlowStep step4 = new DataFlowStep()
            {
                DataFlow = df,
                Action = _datasetContext.QueryStorageAction.FirstOrDefault(),
                DataAction_Type_Id = DataActionType.QueryStorage
            };

            AddDataFlowStep(df, step4);
            _datasetContext.Add(step4);

            _datasetContext.SaveChanges();

            return true;
        }

        public void PublishMessage(string key, string message)
        {
            _messagePublisher.PublishDSCEvent(key, message);
        }

        public IQueryable<DataSourceType> GetDataSourceTypes()
        {
            return _datasetContext.DataSourceTypes;
        }

        public IQueryable<DataSource> GetDataSources()
        {
            return _datasetContext.DataSources;
        }




        #region Private Methods
        private void CreateDataFlow(DataFlowDto dto)
        {           
            DataFlow df = MapToDataFlow(dto);
        }

        private DataFlow MapToDataFlow(DataFlowDto dto)
        {
            DataFlow df = new DataFlow
            {
                Id = dto.Id,
                Name = dto.Name,
                CreatedDTM = DateTime.Now,
                CreatedBy = _userService.GetCurrentUser().AssociateId,
                Questionnaire = dto.DFQuestionnaire
            };

            _datasetContext.Add(df);

            switch (dto.IngestionType)
            {
                case GlobalEnums.IngestionType.User_Push:
                    MapDataFlowStepsForPush(dto, df);
                    break;
                case GlobalEnums.IngestionType.DSC_Pull:
                    break;
                default:
                    break;
            }

            return df;
        }

        private void MapDataFlowStepsForPush(DataFlowDto dto, DataFlow df)
        {
            //This type of dataflow does not need to worry about retrieving data from external sources

            //Generate ingestion steps (get file to raw location)
            MapToS3DropStep(dto, df);
            MapToRawStorageStep(dto, df);

            //Generate preprocessing steps (i.e. uncompress, encoding, etc.)
            //MapPreProcessingSteps(dto, df);
            if (dto.IsCompressed)
            {
                MapToUnCompressStep(dto.CompressionJob, df);
            }

            //Generate DSC registering step
            MapToSchemaLoadStep(dto, df);
            MapToRawQueryStorageStep(dto, df);

            //Generate consumption layer steps
            MapToParquetConverterStep(dto, df);
            
        }

        public void MapToUnCompressStep(CompressionJobDto dto, DataFlow df)
        {
            BaseAction action;
            DataActionType actionType;
            switch (dto.CompressionType)
            {
                case CompressionTypes.ZIP:
                    action = _datasetContext.UncompressZipAction.FirstOrDefault();
                    actionType = DataActionType.UncompressZip;
                    break;
                case CompressionTypes.GZIP:
                default:
                    throw new Exception();
            }

            DataFlowStep step = new DataFlowStep()
            {
                DataFlow = df,
                Action = action,
                DataAction_Type_Id = actionType
            };

            AddDataFlowStep(df, step);
            _datasetContext.Add(step);
        }

        public void MapToParquetConverterStep(DataFlowDto dto, DataFlow df)
        {
            DataFlowStep step = new DataFlowStep()
            {
                DataFlow = df,
                Action = _datasetContext.ConvertToParquetAction.FirstOrDefault(),
                DataAction_Type_Id = DataActionType.ConvertParquet
            };

            AddDataFlowStep(df, step);
            _datasetContext.Add(step);
        }

        public void MapToRawQueryStorageStep(DataFlowDto dto, DataFlow df)
        {
            DataFlowStep step = new DataFlowStep()
            {
                DataFlow = df,
                Action = _datasetContext.QueryStorageAction.FirstOrDefault(),
                DataAction_Type_Id = DataActionType.QueryStorage
            };

            AddDataFlowStep(df, step);
            _datasetContext.Add(step);

        }

        private void MapToSchemaLoadStep(DataFlowDto dto, DataFlow df)
        {
            DataFlowStep step = new DataFlowStep()
            {
                DataFlow = df,
                Action = _datasetContext.SchemaLoadAction.FirstOrDefault(),
                DataAction_Type_Id = DataActionType.SchemaLoad
            };

            AddDataFlowStep(df, step);

            foreach (SchemaMapDto mapDto in dto.SchemaMap)
            {
                MapToSchemaMap(mapDto, step);
            }
        }

        private void MapToSchemaMap(SchemaMapDto dto, DataFlowStep step)
        {
            SchemaMap map =  new SchemaMap()
            {
                DataFlowStepId = step,
                MappedSchema = _datasetContext.FileSchema.Where(w => w.SchemaId == dto.SchemaId).FirstOrDefault(),
                Dataset = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == dto.SchemaId).Select(s => s.ParentDataset).FirstOrDefault(),
                SearchCriteria = dto.SearchCriteria
            };

            _datasetContext.Add(map);

            //step.SchemaMappings.Add(map);
        }

        private void MapToRawStorageStep(DataFlowDto dto, DataFlow df)
        {
            DataFlowStep step = new DataFlowStep()
            {
                DataFlow = df,
                Action = _datasetContext.RawStorageAction.FirstOrDefault(),
                DataAction_Type_Id = DataActionType.RawStorage
            };

            AddDataFlowStep(df, step);
            _datasetContext.Add(step);
        }

        private void MapToS3DropStep(DataFlowDto dto, DataFlow df)
        {
            DataFlowStep step = new DataFlowStep()
            {
                DataFlow = df,
                Action = _datasetContext.S3DropAction.FirstOrDefault(),
                DataAction_Type_Id = DataActionType.S3Drop
            };

            AddDataFlowStep(df, step);
            _datasetContext.Add(step);
        }

        private void MapToDtoList(List<DataFlow> dfList, List<DataFlowDto> dtoList)
        {
            foreach (DataFlow df in dfList)
            {
                DataFlowDto dfDto = new DataFlowDto();
                MapToDto(df, dfDto);
                dtoList.Add(dfDto);
            }
        }

        private void MapToDto(DataFlow df, DataFlowDto dto)
        {
            dto.Id = df.Id;
            dto.FlowGuid = df.FlowGuid;
            dto.Name = df.Name;
            dto.CreateDTM = df.CreatedDTM;
            dto.CreatedBy = df.CreatedBy;
        }

        private void MapToDetailDto(DataFlow flow, DataFlowDetailDto dto)
        {
            List<DataFlowStepDto> stepDtoList = new List<DataFlowStepDto>();
            MapToDtoList(flow.Steps.ToList(), stepDtoList);

            dto.steps = stepDtoList;

            MapToDto(flow, dto);

        }

        private void MapToDto(DataFlowStep step, DataFlowStepDto dto)
        {
            dto.Id = step.Id;
            dto.ActionId = step.Action.Id;
            dto.DataActionType = step.DataAction_Type_Id;
            dto.ExeuctionOrder = step.ExeuctionOrder;
            dto.ActionName = step.Action.Name;
            dto.TriggerKey = step.TriggerKey;
            dto.TargetPrefix = step.Action.TargetStoragePrefix;
        }

        private void MapToDtoList(List<DataFlowStep> steps, List<DataFlowStepDto> dtoList)
        {
            foreach (DataFlowStep step in steps)
            {
                DataFlowStepDto stepDto = new DataFlowStepDto();
                MapToDto(step, stepDto);
                dtoList.Add(stepDto);
            }
        }

        private void AddDataFlowStep(DataFlow df, DataFlowStep step)
        {
            if (df.Steps == null)
            {
                df.Steps = new List<DataFlowStep>();
            }

            //not the first step in list, get previous step to determine trigger
            SetTriggerKey(step, df.Steps.OrderByDescending(o => o.ExeuctionOrder).Take(1).FirstOrDefault());
            SetTargetPrefix(step);

            //Set exeuction order
            step.ExeuctionOrder = df.Steps.Count + 1;

            //Add to DataFlow
            df.Steps.Add(step);
        }

        private void SetTargetPrefix(DataFlowStep step)
        {
            step.TargetPrefix = $"{step.Action.TargetStoragePrefix}{step.DataFlow.Id}/";
        }

        private string GetTargetKey(DataFlowStep step)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(step.Action.TargetStoragePrefix);
            sb.Append(step.DataFlow.Id + "/");
            return sb.ToString();
        }

        private void SetTriggerKey(DataFlowStep step, DataFlowStep previousStep)
        {
            if (previousStep == null)
            {
                switch (step.DataAction_Type_Id)
                {
                    case DataActionType.S3Drop:
                        step.TriggerKey = $"{step.DataFlow.Id}/";
                        break;
                    //case DataActionType.None:
                    //case DataActionType.RawStorage:
                    //case DataActionType.QueryStorage:
                    //case DataActionType.SchemaLoad:
                    //case DataActionType.ConvertParquet:
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                step.TriggerKey = GetTargetKey(previousStep);
            }

        }

        #region SchemaFlowMappings
        public void CreateDataFlowForSchema(FileSchema scm)
        {
            DataFlow df = MapToDataFlow(scm);
        }

        private DataFlow MapToDataFlow(FileSchema scm)
        {
            DataFlow df = new DataFlow
            {
                Id = 0,
                Name = GenerateDataFlowNameForFileSchema(scm),
                CreatedDTM = DateTime.Now,
                CreatedBy = _userService.GetCurrentUser().AssociateId
            };

            _datasetContext.Add(df);

            MapDataFlowStepsForFileSchema(scm, df);

            return df;
        }

        private string GenerateDataFlowNameForFileSchema(FileSchema scm)
        {
            return $"FileSchemaFlow_{scm.SchemaId}_{scm.StorageCode}";
        }


        private void MapDataFlowStepsForFileSchema(FileSchema scm, DataFlow df)
        {
            //Add default DFS drop location for data flow
            RetrieverJob dfsDataFlowBasic = _jobService.InstantiateJobsForCreation(scm, _datasetContext.DataSources.First(x => x.Name.Contains(GlobalConstants.DataSourceName.DEFAULT_DATAFLOW_DFS_DROP_LOCATION)));
            dfsDataFlowBasic.DataFlowId
            _datasetContext.Add(dfsDataFlowBasic);

            //Generate ingestion steps (get file to raw location)
            MapToS3DropStep(null, df);
            MapToRawStorageStep(null, df);

            //Generate preprocessing steps (i.e. uncompress, encoding, etc.)
            //MapPreProcessingSteps(dto, df);
            //if (dto.IsCompressed)
            //{
            //    MapToUnCompressStep(dto.CompressionJob, df);
            //}

            //Generate DSC registering step
            MapToSchemaLoadStep(MapToDto(scm), df);
            MapToRawQueryStorageStep(null, df);

            ////Generate consumption layer steps
            //MapToParquetConverterStep(dto, df);

        }

        private DataFlowDto MapToDto(FileSchema scm)
        {
            return new DataFlowDto()
            {
                SchemaMap = new List<SchemaMapDto>()
                {
                    new SchemaMapDto()
                    {
                        SchemaId = scm.SchemaId,
                        SearchCriteria = "\\."
                    }
                }
            };
        }
        #endregion

        #endregion
    }
}
