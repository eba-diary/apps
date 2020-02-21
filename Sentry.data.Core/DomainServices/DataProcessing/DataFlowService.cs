using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
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

        //public bool CreateDataFlow(int schemaId)
        //{
        //    int cnt = _datasetContext.DataFlow.Count();
        //    DataFlow df = new DataFlow()
        //    {
        //        Name = "CreateDataFlowTest_" + cnt.ToString(),
        //        CreatedBy = "072984",
        //        CreatedDTM = DateTime.Now,
        //    };

        //    _datasetContext.Add(df);

        //    DataFlowStep step1 = new DataFlowStep()
        //    {
        //        DataFlow = df,
        //        Action = _datasetContext.S3DropAction.FirstOrDefault(),
        //        DataAction_Type_Id = DataActionType.S3Drop
        //    };

        //    AddDataFlowStep(df, step1);
        //    _datasetContext.Add(step1);

        //    DataFlowStep step3 = new DataFlowStep()
        //    {
        //        DataFlow = df,
        //        Action = _datasetContext.RawStorageAction.FirstOrDefault(),
        //        DataAction_Type_Id = DataActionType.RawStorage
        //    };

        //    AddDataFlowStep(df, step3);
        //    _datasetContext.Add(step3);

        //    DataFlowStep step2 = new DataFlowStep()
        //    {
        //        DataFlow = df,
        //        Action = _datasetContext.SchemaLoadAction.FirstOrDefault(),
        //        DataAction_Type_Id = DataActionType.SchemaLoad
        //    };

        //    AddDataFlowStep(df, step2);
        //    _datasetContext.Add(step2);

        //    SchemaMap mapping = new SchemaMap()
        //    {
        //        DataFlowStepId = step2,
        //        MappedSchema = _datasetContext.FileSchema.Where(w => w.SchemaId == schemaId).FirstOrDefault(),
        //        Dataset = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schemaId).Select(s => s.ParentDataset).FirstOrDefault(),
        //        SearchCriteria = "Testfile.csv"
        //    };
        //    _datasetContext.Add(mapping);
        //    List<SchemaMap> maps = new List<SchemaMap>();
        //    maps.Add(mapping);
        //    step2.SchemaMappings = maps;

        //    DataFlowStep step4 = new DataFlowStep()
        //    {
        //        DataFlow = df,
        //        Action = _datasetContext.QueryStorageAction.FirstOrDefault(),
        //        DataAction_Type_Id = DataActionType.QueryStorage
        //    };

        //    AddDataFlowStep(df, step4);
        //    _datasetContext.Add(step4);

        //    _datasetContext.SaveChanges();

        //    return true;
        //}

        public void CreateDataFlowForSchema(FileSchema scm)
        {
            DataFlow df = MapToDataFlow(scm);

            MapDataFlowStepsForFileSchema(scm, df);
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

        public string GetDataFlowNameForFileSchema(FileSchema scm)
        {
            string dataFlowName = GenerateDataFlowNameForFileSchema(scm);
            return dataFlowName;
        }

        public DataFlowStep GetS3DropStepForFileSchema(FileSchema scm)
        {
            if(scm == null)
            {
                throw new ArgumentNullException("FileSchema is required");
            }

            string schemaFlowName = GenerateDataFlowNameForFileSchema(scm);
            DataFlow flow = _datasetContext.DataFlow.Where(w => w.Name == schemaFlowName).FirstOrDefault();
            DataFlowStep step = _datasetContext.DataFlowStep.Where(w => w.DataFlow == flow && w.DataAction_Type_Id == DataActionType.S3Drop).FirstOrDefault();

            if (step == null)
            {
                throw new DataFlowStepNotFound();
            }

            return step;
        }

        public DataFlow GetDataFlowByName(string schemaFlowName)
        {
            if (string.IsNullOrEmpty(schemaFlowName))
            {
                throw new ArgumentNullException("schemaFlowName not specified");
            }

            DataFlow flow = _datasetContext.DataFlow.Where(w => w.Name == schemaFlowName).FirstOrDefault();

            if (flow == null)
            {
                throw new DataFlowNotFound();
            }

            return flow;
        }

        public DataFlowStep GetDataFlowStepForDataFlowByActionType(int dataFlowId, DataActionType actionType)
        {
            if (dataFlowId == 0)
            {
                throw new ArgumentNullException("dataFlowId is not specified");
            }
            if (actionType == DataActionType.None)
            {
                throw new ArgumentNullException("actionType is not specified");
            }

            DataFlowStep step = _datasetContext.DataFlowStep.Where(w => w.DataFlow.Id == dataFlowId && w.DataAction_Type_Id == actionType).FirstOrDefault();
            if (step == null)
            {
                throw new DataFlowStepNotFound();
            }

            return step;
        }

        public List<DataFlowStep> GetDependentDataFlowStepsForDataFlowStep(int stepId)
        {
            if (stepId == 0)
            {
                throw new ArgumentNullException("DataFlowStep is required");
            }

            /****************************************************
             * Retrieve current step
             ****************************************************/
            DataFlowStep step = _datasetContext.DataFlowStep.Where(w => w.Id == stepId).FirstOrDefault();

            List<DataFlowStep> steps = new List<DataFlowStep>();

            /****************************************************
             * Find all downstream dependent steps based on:
             *   - Current step trigger key equals to dependent step SourceDependencyPrefix
             *   - Current step bucket equals to dependent step SourceDependencyBucket
             ****************************************************/
            steps = _datasetContext.DataFlowStep.Where(w => w.SourceDependencyPrefix == step.TriggerKey && w.SourceDependencyBucket == step.Action.TargetStorageBucket).ToList();

            return steps;
        }

        public string GetSchemaStorageCodeForDataFlow(int dataFlowId)
        {
            DataFlowStep schemaLoadStep = GetDataFlowStepForDataFlowByActionType(dataFlowId, DataActionType.SchemaLoad);

            if (schemaLoadStep == null)
            {
                return null;
            }

            return schemaLoadStep.SchemaMappings.Single().MappedSchema.StorageCode;

        }

        #region Private Methods
        private void CreateDataFlow(DataFlowDto dto)
        {           
            DataFlow df = MapToDataFlow(dto);

            switch (dto.IngestionType)
            {
                case GlobalEnums.IngestionType.User_Push:
                    MapDataFlowStepsForPush(dto, df);
                    break;
                case GlobalEnums.IngestionType.DSC_Pull:
                    MapDataFlowStepsForPull(dto, df);
                    break;
                default:
                    break;
            }
        }

        private DataFlow MapToDataFlow(DataFlowDto dto)
        {
            DataFlow df = new DataFlow
            {
                Id = dto.Id,
                Name = dto.Name,
                CreatedDTM = DateTime.Now,
                CreatedBy = _userService.GetCurrentUser().AssociateId,
                Questionnaire = dto.DFQuestionnaire,
                FlowStorageCode = _datasetContext.GetNextDataFlowStorageCDE()
            };

            _datasetContext.Add(df);

            return df;
        }

        private void MapDataFlowStepsForPush(DataFlowDto dto, DataFlow df)
        {
            //This type of dataflow does not need to worry about retrieving data from external sources
            // Data will be pushed by user to S3 and\or DFS drop locations

            //Generate ingestion steps (get file to raw location)
            AddDataFlowStep(dto, df, DataActionType.S3Drop);

            //MapToRawStorageStep(dto, df);
            AddDataFlowStep(dto, df, DataActionType.RawStorage);

            if (dto.IsCompressed)
            {
                switch (dto.CompressionJob.CompressionType)
                {
                    case CompressionTypes.ZIP:
                        AddDataFlowStep(dto, df, DataActionType.UncompressZip);
                        break;
                    case CompressionTypes.GZIP:
                        AddDataFlowStep(dto, df, DataActionType.UncompressGZip);
                        break;
                    default:
                        break;
                }
            }

            if (dto.IsPreProcessingRequired)
            {
                foreach (DataFlowPreProcessingTypes item in dto.PreProcessingOptions)
                {
                    switch (item)
                    {
                        case DataFlowPreProcessingTypes.googleapi:
                            AddDataFlowStep(dto, df, DataActionType.GoogleApi);
                            break;
                        case DataFlowPreProcessingTypes.claimiq:
                            AddDataFlowStep(dto, df, DataActionType.ClaimIq);
                            break;
                        default:
                            break;
                    }
                }
            }

            //Generate Schema Map step to send files to schema specific data flow
            AddDataFlowStep(dto, df, DataActionType.SchemaMap);
        }

        private void MapDataFlowStepsForPull(DataFlowDto dto, DataFlow df)
        {
            //RetrieverJobDto jobDto = new RetrieverJobDto();
            //MaptToDto(dto, jobDto);
            //jobDto.DataFlow = df.Id;
            dto.RetrieverJob.DataFlow = df.Id;
            _jobService.CreateAndSaveRetrieverJob(dto.RetrieverJob);

            //Generate ingestion steps (get file to raw location)
            AddDataFlowStep(dto, df, DataActionType.S3Drop);

            //MapToRawStorageStep(dto, df);
            AddDataFlowStep(dto, df, DataActionType.RawStorage);

            if (dto.IsCompressed)
            {
                switch (dto.CompressionJob.CompressionType)
                {
                    case CompressionTypes.ZIP:
                        AddDataFlowStep(dto, df, DataActionType.UncompressZip);
                        break;
                    case CompressionTypes.GZIP:
                        AddDataFlowStep(dto, df, DataActionType.UncompressGZip);
                        break;
                    default:
                        break;
                }
            }

            if (dto.IsPreProcessingRequired)
            {
                foreach (DataFlowPreProcessingTypes item in dto.PreProcessingOptions)
                {
                    switch (item)
                    {
                        case DataFlowPreProcessingTypes.googleapi:
                            AddDataFlowStep(dto, df, DataActionType.GoogleApi);
                            break;
                        case DataFlowPreProcessingTypes.claimiq:
                            AddDataFlowStep(dto, df, DataActionType.ClaimIq);
                            break;
                        default:
                            break;
                    }
                }
            }

            //Generate Schema Map step to send files to schema specific data flow
            AddDataFlowStep(dto, df, DataActionType.SchemaMap);

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

        private void MaptToDto(DataFlowDto dto, RetrieverJobDto jobDto)
        {
            jobDto.DataSourceId = dto.RetrieverJob.DataSourceId;
            jobDto.DataSourceType = dto.RetrieverJob.DataSourceType;
            jobDto.IsCompressed = false; //for the data flow compression is handled outside of retriever job logic
            jobDto.CreateCurrentFile = dto.RetrieverJob.CreateCurrentFile;
            jobDto.DatasetFileConfig = 0; //jobs for the data flow are linked via data flow id not datasetfileconfig
            jobDto.FileNameExclusionList = dto.RetrieverJob.FileNameExclusionList;
            jobDto.FileSchema = dto.RetrieverJob.FileSchema;
            jobDto.FtpPatrn = dto.RetrieverJob.FtpPatrn;
            jobDto.HttpRequestBody = dto.RetrieverJob.HttpRequestBody;
            jobDto.JobId = dto.RetrieverJob.JobId;
            jobDto.RelativeUri = dto.RetrieverJob.RelativeUri;
            jobDto.RequestDataFormat = dto.RetrieverJob.RequestDataFormat;
            jobDto.RequestMethod = dto.RetrieverJob.RequestMethod;
            jobDto.Schedule = dto.RetrieverJob.Schedule;
            jobDto.SearchCriteria = dto.RetrieverJob.SearchCriteria;
            jobDto.TargetFileName = dto.RetrieverJob.TargetFileName;
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

        private void AddDataFlowStep(DataFlowDto dto, DataFlow df, DataActionType actionType)
        {
            DataFlowStep step = CreateDataFlowStep(actionType, dto, df);            

            if (df.Steps == null)
            {
                df.Steps = new List<DataFlowStep>();
            }

            SetTriggerPrefix(step);
            SetTargetPrefix(step);
            SetSourceDependency(step, df.Steps.OrderByDescending(o => o.ExeuctionOrder).Take(1).FirstOrDefault());

            //Set exeuction order
            step.ExeuctionOrder = df.Steps.Count + 1;

            //Add to DataFlow
            df.Steps.Add(step);
        }

        private DataFlowStep CreateDataFlowStep(DataActionType actionType, DataFlowDto dto, DataFlow df)
        {
            BaseAction action;
            switch (actionType)
            {                
                case DataActionType.S3Drop:
                    action = _datasetContext.S3DropAction.FirstOrDefault();
                    actionType = DataActionType.S3Drop;
                    break;
                case DataActionType.RawStorage:
                    action = _datasetContext.RawStorageAction.FirstOrDefault();
                    actionType = DataActionType.RawStorage;
                    break;
                case DataActionType.QueryStorage:
                    action = _datasetContext.QueryStorageAction.FirstOrDefault();
                    actionType = DataActionType.QueryStorage;
                    break;
                case DataActionType.ConvertParquet:
                    action = _datasetContext.ConvertToParquetAction.FirstOrDefault();
                    actionType = DataActionType.ConvertParquet;
                    break;
                case DataActionType.UncompressZip:
                    action = _datasetContext.UncompressZipAction.FirstOrDefault();
                    actionType = DataActionType.UncompressZip;
                    break;
                case DataActionType.GoogleApi:
                    action = _datasetContext.GoogleApiAction.FirstOrDefault();
                    actionType = DataActionType.GoogleApi;
                    break;
                case DataActionType.ClaimIq:
                    action = _datasetContext.ClaimIQAction.FirstOrDefault();
                    actionType = DataActionType.ClaimIq;
                    break;
                case DataActionType.SchemaLoad:
                    action = _datasetContext.SchemaLoadAction.FirstOrDefault();
                    actionType = DataActionType.SchemaLoad;
                    DataFlowStep schemaLoadStep = MapToDataFlowStep(df, action, actionType);
                    foreach (SchemaMapDto mapDto in dto.SchemaMap)
                    {
                        MapToSchemaMap(mapDto, schemaLoadStep);
                    }
                    return schemaLoadStep;
                case DataActionType.SchemaMap:
                    action = _datasetContext.SchemaMapAction.FirstOrDefault();
                    actionType = DataActionType.SchemaMap;
                    DataFlowStep schemaMapStep = MapToDataFlowStep(df, action, actionType);
                    foreach (SchemaMapDto mapDto in dto.SchemaMap)
                    {
                        MapToSchemaMap(mapDto, schemaMapStep);
                        bool exists = DataFlowExistsForFileSchema(mapDto.SchemaId);
                        if (!exists)
                        {
                            Logger.Debug("dataflowservice-createdataflowstep fileschema-dataflow-not-detected");
                            Logger.Debug("dataflowservice-createdataflowstep creating-fileschema-dataflow");
                            FileSchema scm = _datasetContext.GetById<FileSchema>(mapDto.SchemaId);
                            CreateDataFlowForSchema(scm);
                        }
                    }
                    return schemaMapStep;
                case DataActionType.UncompressGZip:
                case DataActionType.None:
                default:
                    return null;
            }

            return MapToDataFlowStep(df, action, actionType);
        }

        private bool DataFlowExistsForFileSchema(int schemaId)
        {
            FileSchema scm = _datasetContext.GetById<FileSchema>(schemaId);
            var schemaFlowName = GetDataFlowNameForFileSchema(scm);
            bool exists = _datasetContext.DataFlow.Any(w => w.Name == schemaFlowName);
            return exists;
        }

        private DataFlowStep MapToDataFlowStep(DataFlow df, BaseAction action, DataActionType actionType)
        {
            DataFlowStep step = new DataFlowStep()
            {
                DataFlow = df,
                Action = action,
                DataAction_Type_Id = actionType
            };

            _datasetContext.Add(step);

            return step;
        }

        private void SetTriggerPrefix(DataFlowStep step)
        {
            if(step.DataAction_Type_Id == DataActionType.S3Drop)
            {
                step.TriggerKey = $"droplocation/{Configuration.Config.GetHostSetting("S3DataPrefix")}{step.DataFlow.FlowStorageCode}/";
            }
            else
            {
                step.TriggerKey = $"{GlobalConstants.DataFlowTargetPrefixes.TEMP_FILE_PREFIX}{step.Action.TargetStoragePrefix}{step.DataFlow.FlowStorageCode}/";
            }            
        }

        private string GetTargetKey(DataFlowStep step)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(step.Action.TargetStoragePrefix);
            sb.Append(step.DataFlow.FlowStorageCode + "/");
            return sb.ToString();
        }

        private void SetTargetPrefix(DataFlowStep step)
        {
            switch (step.DataAction_Type_Id)
            {
                case DataActionType.None:
                    //break;
                    //step.TargetPrefix = $"{GlobalConstants.DataFlowTargetPrefixes.TEMP_FILE_PREFIX}" + step.Action.TargetStoragePrefix + $"{step.DataFlow.Id}/";
                    break;
                //These sent output a step specific location along with down stream dependent steps
                case DataActionType.RawStorage:
                case DataActionType.QueryStorage:
                case DataActionType.ConvertParquet:
                    step.TargetPrefix = step.Action.TargetStoragePrefix + $"{step.DataFlow.FlowStorageCode}/";
                    break;
                //These only send output to down stream dependent steps
                case DataActionType.SchemaLoad:
                case DataActionType.UncompressZip:
                case DataActionType.UncompressGZip:
                case DataActionType.SchemaMap:
                case DataActionType.S3Drop:
                    step.TargetPrefix = null;
                    break;
                default:
                    break;
            }
        }

        private void SetSourceDependency(DataFlowStep step, DataFlowStep previousStep)
        {
            step.SourceDependencyPrefix = previousStep?.TriggerKey;
            step.SourceDependencyBucket = previousStep?.Action.TargetStorageBucket;
        }

        #region SchemaFlowMappings

        private DataFlow MapToDataFlow(FileSchema scm)
        {
            DataFlow df = new DataFlow
            {
                Id = 0,
                Name = GenerateDataFlowNameForFileSchema(scm),
                CreatedDTM = DateTime.Now,
                CreatedBy = _userService.GetCurrentUser().AssociateId,
                FlowStorageCode = _datasetContext.GetNextDataFlowStorageCDE()
            };

            _datasetContext.Add(df);

            return df;
        }

        private string GenerateDataFlowNameForFileSchema(FileSchema scm)
        {
            return $"FileSchemaFlow_{scm.SchemaId}_{scm.StorageCode}";
        }

        private void MapDataFlowStepsForFileSchema(FileSchema scm, DataFlow df)
        {
            DataFlowDto dto = MapToDto(scm);

            //Add default DFS drop location for data flow
            RetrieverJob dfsDataFlowBasic = _jobService.InstantiateJobsForCreation(df, _datasetContext.DataSources.First(x => x.Name.Contains(GlobalConstants.DataSourceName.DEFAULT_DATAFLOW_DFS_DROP_LOCATION)));
            _datasetContext.Add(dfsDataFlowBasic);

            //Generate ingestion steps (get file to raw location)
            AddDataFlowStep(dto, df, DataActionType.S3Drop);
            AddDataFlowStep(dto, df, DataActionType.RawStorage);

            //Generate preprocessing for file types (i.e. csv, json, etc...)
            //MapPreProcessingSteps(dto, df);

            //Generate DSC registering step
            AddDataFlowStep(dto, df, DataActionType.SchemaLoad);
            AddDataFlowStep(dto, df, DataActionType.QueryStorage);

            ////Generate consumption layer steps
            AddDataFlowStep(dto, df, DataActionType.ConvertParquet);

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
