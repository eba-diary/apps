using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sentry.data.Core
{
    public class DataFlowService : IDataFlowService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IMessagePublisher _messagePublisher;
        private readonly UserService _userService;
        private readonly IJobService _jobService;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly ISecurityService _securityService;


        public DataFlowService(IDatasetContext datasetContext, IMessagePublisher messagePublisher, 
            UserService userService, IJobService jobService, IS3ServiceProvider s3ServiceProvider,
            ISecurityService securityService)
        {
            _datasetContext = datasetContext;
            _messagePublisher = messagePublisher;
            _userService = userService;
            _jobService = jobService;
            _s3ServiceProvider = s3ServiceProvider;
            _securityService = securityService;
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

        public int CreateandSaveDataFlow(DataFlowDto dto)
        {
            //Verify user has permissions to create Dataflow
            UserSecurity us = _securityService.GetUserSecurity(null, _userService.GetCurrentUser());

            if (!us.CanCreateDataFlow)
            {
                throw new DataFlowUnauthorizedAccessException();
            }

            //Verify user has permissions to push data to each schema mapped to dataflow
            StringBuilder datasetsWithNoPermissions = new StringBuilder();
            foreach(SchemaMapDto scmMap in dto.SchemaMap)
            {
                Dataset ds = _datasetContext.GetById<Dataset>(scmMap.DatasetId);
                bool IsDatasetDetected = datasetsWithNoPermissions.ToString().Split(',').Any(a => a == ds.DatasetName);

                UserSecurity dsUs = null;
                //If dataset name is already in list do not retrieve security object again
                if (!IsDatasetDetected)
                {
                    dsUs = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
                }

                //Only check permissions if security object is populated.  Also prevents
                //  returning list with dataset name listed multiple times.
                if (dsUs != null && !dsUs.CanManageSchema && !dsUs.CanEditDataset)
                {
                    if (datasetsWithNoPermissions.Length > 0)
                    {
                        datasetsWithNoPermissions.Append($", {ds.DatasetName}");
                    }
                    else
                    {
                        datasetsWithNoPermissions.Append($"{ds.DatasetName}");
                    }                    
                }
            }

            //If SchemaMapDto contains dataset which user does not have permissions to 
            // push data too, then throw an unauthorized exception.
            if (datasetsWithNoPermissions.Length > 0)
            {
                throw new DatasetUnauthorizedAccessException($"No permissions to push data to {datasetsWithNoPermissions}");
            }

            try
            {
                DataFlow df = CreateDataFlow(dto);

                _datasetContext.SaveChanges();

                return df.Id;
            }
            catch (ValidationException)
            {
                //throw validation errors for controller to handle
                _datasetContext.Clear();
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("dataflowservice-createandsavedataflow failed to save dataflow", ex);
                return 0;
            }
        }

        public void CreateDataFlowForSchema(FileSchema scm)
        {
            DataFlow df = MapToDataFlow(scm);

            MapDataFlowStepsForFileSchema(scm, df);

            _jobService.CreateDropLocation(_datasetContext.RetrieverJob.FirstOrDefault(w => w.DataFlow == df));
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

        public DataFlowStepDto GetS3DropStepForFileSchema(FileSchema scm)
        {
            if(scm == null)
            {
                throw new ArgumentNullException("scm", "FileSchema is required");
            }

            string schemaFlowName = GenerateDataFlowNameForFileSchema(scm);
            DataFlow flow = _datasetContext.DataFlow.Where(w => w.Name == schemaFlowName).FirstOrDefault();
            DataFlowStep step = _datasetContext.DataFlowStep.Where(w => w.DataFlow == flow && (w.DataAction_Type_Id == DataActionType.S3Drop || w.DataAction_Type_Id == DataActionType.ProducerS3Drop)).FirstOrDefault();
            if (step == null)
            {
                throw new DataFlowStepNotFound();
            }
            DataFlowStepDto stepDto = new DataFlowStepDto();
            MapToDto(step, stepDto);

            

            return stepDto;
        }

        public DataFlow GetDataFlowByName(string schemaFlowName)
        {
            if (string.IsNullOrEmpty(schemaFlowName))
            {
                throw new ArgumentNullException("schemaFlowName", "schemaFlowName not specified");
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
                throw new ArgumentNullException("dataFlowId", "dataFlowId is not specified");
            }
            if (actionType == DataActionType.None)
            {
                throw new ArgumentNullException("actionType", "actionType is not specified");
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
                throw new ArgumentNullException("stepId", "DataFlowStep is required");
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

        public string GetSchemaStorageCodeForDataFlow(int Id)
        {
            DataFlowStep schemaLoadStep = GetDataFlowStepForDataFlowByActionType(Id, DataActionType.SchemaLoad);

            if (schemaLoadStep == null)
            {
                return null;
            }

            return schemaLoadStep.SchemaMappings.Single().MappedSchema.StorageCode;

        }

        public void DeleteByFileSchema(FileSchema scm)
        {
            //Find schema specific dataflow
            DataFlow schemaSpecificFlow = _datasetContext.DataFlow.Where(w => w.Name == GenerateDataFlowNameForFileSchema(scm)).FirstOrDefault();

            //Find all SchemaMappings associated with FileSchema
            List<SchemaMap> schemaMappings = _datasetContext.SchemaMap.Where(w => w.MappedSchema == scm).ToList();

            foreach (SchemaMap map in schemaMappings)
            {
                //ScheamMap step associated with SchemaMapping
                DataFlowStep mapStep = map.DataFlowStepId;

                //Find non-SchemaSpecific DataFlow associated with SchemaMap step, then return all SchemaMap steps associated with DataFlow
                List<DataFlowStep> associatedMapSteps =  _datasetContext.GetById<DataFlow>(mapStep.DataFlow.Id).Steps.Where(w => w.DataAction_Type_Id == DataActionType.SchemaMap).ToList();

                //if non-SchemaSpecific flow only contains 1 SchemaMap step, issue delete of DataFlow
                // if count greater than 1 and all other SchemaMaps reference same FileSchema, issue delete of DataFlow
                // if count greater than 1 and any other SchemaMaps reference differnt FileSchema, only delete SchemaMap step

                //There are no non-SchemaSpecific dataflows mapped to this schema,
                //  Therefore, delete schema specific dataflow
                if (associatedMapSteps.Count == 0)
                {                    
                    Delete(mapStep.DataFlow.Id);
                }
                else if (associatedMapSteps.Count >= 1)
                {
                    int nonMatchingFileSchemas = 0;
                    foreach (DataFlowStep step in associatedMapSteps)
                    {
                        if (scm.SchemaId != _datasetContext.SchemaMap.Where(w => w.DataFlowStepId == step).Select(s => s.MappedSchema).FirstOrDefault().SchemaId)
                        {
                            nonMatchingFileSchemas++;
                        }
                    }

                    if (nonMatchingFileSchemas == 0)
                    {
                        Delete(mapStep.DataFlow.Id);
                    }
                    else
                    {
                        _datasetContext.Remove(map);
                    }
                }
            }

            // Issue Delete of Schema-Specific data flow
            Delete(schemaSpecificFlow.Id);

        }

        public void DeleteFlowsByFileSchema(FileSchema scm, bool logicalDelete = true)
        {
            string methodName = MethodBase.GetCurrentMethod().Name.ToLower();
            Logger.Debug($"Start method <{methodName}>");


            /* Get Schema Flow */
            var schemaflowName = GetDataFlowNameForFileSchema(scm);
            DataFlow schemaFlow = _datasetContext.DataFlow.FirstOrDefault(w => w.Name == schemaflowName);

            //some legacy dataset\schema may not have associated schema flow
            if (schemaFlow == null)
            {
                Logger.Debug($"Schema Flow not found by name, attempting to detect by id...");

                SchemaMap mappedStep = _datasetContext.SchemaMap.SingleOrDefault(w => w.MappedSchema.SchemaId == scm.SchemaId && w.DataFlowStepId.Action.Name == "Schema Load");
                if (mappedStep != null)
                {
                    Logger.Debug($"detected schema flow by Id");
                    schemaFlow = _datasetContext.DataFlowStep.FirstOrDefault(w => w.Id == mappedStep.Id).DataFlow;
                }
                else
                {
                    Logger.Debug($"schema flow not detected by id");
                    Logger.Debug($"no schema flow associated with schema");
                    Logger.Debug($"End method <{methodName}>");
                    return;
                }
            }

            Logger.Debug($"schema flow name: {schemaFlow.Name}");

            if (logicalDelete)
            {

                /* Mark dataflow for deletion */
                schemaFlow.ObjectStatus = GlobalEnums.ObjectStatusEnum.Pending_Delete;
                schemaFlow.DeleteIssuer = _userService.GetCurrentUser().AssociateId;
                schemaFlow.DeleteIssueDTM = DateTime.Now;


                /* Get any Retriever Jobs associated with schema flow */
                List<int> schemaFlowJobList = new List<int>();
                schemaFlowJobList = _datasetContext.RetrieverJob.Where(w => w.DataFlow == schemaFlow).Select(s => s.Id).ToList();

                if (schemaFlowJobList.Any())
                {
                    Logger.Debug($"{methodName} - detected jobs for schema dataflow - count:{schemaFlowJobList.Count.ToString()}:::jobids:{("[{0}]", string.Join(", ", schemaFlowJobList.Select(e => e.ToString()).ToArray()))}");

                    foreach (int jobId in schemaFlowJobList)
                    {
                        _jobService.DisableJob(jobId);
                    }
                }
                else
                {
                    Logger.Debug($"no jobs detected for schema dataflow");
                }


                /* Get Producer flows associated with Schema flow */
                List<int> producerDataflowIdForDisableList = GetProducerFlowsToBeDisabledBySchemaId(scm.SchemaId);

                if (producerDataflowIdForDisableList.Any())
                {
                    Logger.Debug($"{methodName} - detected producer flows associated with  - count:{producerDataflowIdForDisableList.Count.ToString()}:::jobids:{("[{0}]", string.Join(", ", producerDataflowIdForDisableList.Select(e => e.ToString()).ToArray()))}");

                    /* Mark Dataflow for deletion */
                    foreach(int dataFlowId in producerDataflowIdForDisableList)
                    {
                        DataFlow producerFlow = _datasetContext.DataFlow.FirstOrDefault(w => w.Id == dataFlowId);
                        producerFlow.ObjectStatus = GlobalEnums.ObjectStatusEnum.Pending_Delete;
                        producerFlow.DeleteIssuer = _userService.GetCurrentUser().AssociateId;
                        producerFlow.DeleteIssueDTM = DateTime.Now;
                    }


                    /* Disable all retrieverJobs associated with producer flow */
                    List<int> producerFlowJobList = _datasetContext.RetrieverJob.Where(w => producerDataflowIdForDisableList.Contains(w.DataFlow.Id)).Select(s => s.Id).ToList();

                    if (producerFlowJobList.Any())
                    {
                        Logger.Debug($"{methodName} - detected jobs for produer dataflow - count:{producerFlowJobList.Count.ToString()}:::jobids:{("[{0}]", string.Join(", ", producerFlowJobList.Select(e => e.ToString()).ToArray()))}");

                        foreach (int jobId in producerFlowJobList)
                        {
                            _jobService.DisableJob(jobId);
                        }
                    }
                    else
                    {
                        Logger.Debug($"no jobs detected for producer dataflow");
                    }
                }
                else
                {
                    Logger.Debug($"no producer dataflows detected");
                }
            }
            else
            {
                // Mark Schema flow deleted
                schemaFlow.ObjectStatus = GlobalEnums.ObjectStatusEnum.Deleted;

                List<int> producerDataflowIdForDeleteList = GetProducerFlowsToBeDisabledBySchemaId(scm.SchemaId);

                Logger.Debug(producerDataflowIdForDeleteList.Any() ? $"detected {producerDataflowIdForDeleteList.Count} producer flows for delete" : $"detected 0 producer flows for delete");

                /* Mark Dataflow for deletion */
                foreach (int dataFlowId in producerDataflowIdForDeleteList)
                {
                    DataFlow producerFlow = _datasetContext.DataFlow.FirstOrDefault(w => w.Id == dataFlowId);
                    Logger.Debug($"marking {producerFlow.Name} producer flow as deleted");
                    producerFlow.ObjectStatus = GlobalEnums.ObjectStatusEnum.Deleted;
                }
            }

            Logger.Debug($"End method <{methodName}>");
        }

        private List<int> GetProducerFlowsToBeDisabledBySchemaId(int schemaId)
        {
            List<int> producerFlowIdList = new List<int>();

            /* Get producer flow Ids which map to schema */
            List<Tuple<int, int>> producerSchemaMapIds = _datasetContext.SchemaMap.Where(w => w.MappedSchema.SchemaId == schemaId && w.DataFlowStepId.DataAction_Type_Id == DataActionType.SchemaMap).Select(s => new Tuple<int, int>( s.DataFlowStepId.Id, s.DataFlowStepId.DataFlow.Id)).ToList();
   
            foreach (Tuple<int, int> item in producerSchemaMapIds)
            {
                /*  Add producer dataflow id to list if the
                 *  dataflow does not map to any other schema
                 *  
                 *  Or add producer dataflow id to list if all
                 *  other mapped schema are NOT Active    
                 */
                if (!_datasetContext.SchemaMap.Any(w => w.DataFlowStepId.Id == item.Item1 && w.MappedSchema.SchemaId != schemaId))
                {
                    producerFlowIdList.Add(item.Item2);
                }
                else if (!_datasetContext.SchemaMap.Any(w => w.DataFlowStepId.Id == item.Item1 && w.MappedSchema.SchemaId != schemaId && w.MappedSchema.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active))
                {
                    producerFlowIdList.Add(item.Item2);
                }
            }

            return producerFlowIdList;            
        }

        public void Delete(int dataFlowId)
        {
            Logger.Info($"dataflowservice-delete-start - dataflowid:{dataFlowId}");

            //Find DataFlow
            DataFlow flow = _datasetContext.GetById<DataFlow>(dataFlowId);

            if (flow == null)
            {
                Logger.Debug($"dataflowservice-delete DataFlow not found - dataflowid:{dataFlowId.ToString()}");
            }
            else
            {
                //Find associated RetrieverJobs
                List<RetrieverJob> jobs = _datasetContext.RetrieverJob.Where(w => w.DataFlow == flow).ToList();

                Logger.Info($"dataflowservice-delete-deleteretrieverjobs - dataflowid:{flow.Id}");
                foreach (RetrieverJob job in jobs)
                {
                    _jobService.DeleteJob(job.Id);
                }

                //Remove long-term storage files                
                DeleteLongTermFiles(flow);

                Logger.Info($"dataflowservice-delete-deletedataflow - dataflowid:{flow.Id}");
                _datasetContext.Remove(flow);
                _datasetContext.SaveChanges();
            }

            Logger.Info($"dataflowservice-delete-end - dataflowid:{dataFlowId}");
        }

        public List<SchemaMapDetailDto> GetMappedSchemaByDataFlow(int dataflowId)
        {
            List<SchemaMap> schemaMapList = _datasetContext.DataFlowStep.Where(w => w.DataFlow.Id == dataflowId).SelectMany(s => s.SchemaMappings).ToList();
            List<SchemaMapDetailDto> dtoList = new List<SchemaMapDetailDto>();
            foreach(SchemaMap map in schemaMapList)
            {
                SchemaMapDetailDto dto = new SchemaMapDetailDto();
                MapToDetailDto(map, dto);
                dtoList.Add(dto);
            }
            return dtoList;
        }

        public ValidationException Validate(DataFlowDto dfDto)
        {
            ValidationResults results = new ValidationResults();
            if (_datasetContext.DataFlow.Any(w => w.Name == dfDto.Name))
            {
                results.Add(DataFlow.ValidationErrors.nameMustBeUnique, "Dataflow name is already used");
            }
            return new ValidationException(results);
        }

        #region Private Methods
        private void DeleteLongTermFiles(DataFlow flow)
        {
            Logger.Info($"dataflowservice-deleteLongtermfiles - dataflowid:{flow.Id}");
            foreach (DataFlowStep step in flow.Steps)
            {
                if (step.DataAction_Type_Id == DataActionType.S3Drop)
                {
                    Logger.Info($"dataflowservice-deleteLongtermfiles-delete - dataflowid:{flow.Id} stepid:{step.Id} prefix:{step.TriggerKey}");
                    _s3ServiceProvider.DeleteS3Prefix(step.TriggerKey);
                }
                else if (step.DataAction_Type_Id == DataActionType.RawStorage)
                {
                    Logger.Info($"dataflowservice-deleteLongtermfiles-delete - dataflowid:{flow.Id} stepid:{step.Id} prefix:{step.TargetPrefix}");
                    _s3ServiceProvider.DeleteS3Prefix(step.TargetPrefix);                    
                }
            }
        }

        private DataFlow CreateDataFlow(DataFlowDto dto)
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

            return df;
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
                FlowStorageCode = (string.IsNullOrEmpty(dto.FlowStorageCode)) ? _datasetContext.GetNextDataFlowStorageCDE() : dto.FlowStorageCode,
                SaidKeyCode = dto.SaidKeyCode,
                ObjectStatus = Core.GlobalEnums.ObjectStatusEnum.Active,
                DeleteIssuer = dto.DeleteIssuer,
                DeleteIssueDTM = DateTime.MaxValue
            };

            _datasetContext.Add(df);

            return df;
        }

        private void MapDataFlowStepsForPush(DataFlowDto dto, DataFlow df)
        {
            //This type of dataflow does not need to worry about retrieving data from external sources
            // Data will be pushed by user to S3 and\or DFS drop locations
            //Add default DFS drop location for data flow
            List<DataSource> srcList = _datasetContext.DataSources.ToList();
            RetrieverJob dfsDataFlowBasic = _jobService.InstantiateJobsForCreation(df, srcList.First(w => w.SourceType == GlobalConstants.DataSoureDiscriminator.DEFAULT_DATAFLOW_DFS_DROP_LOCATION));
            _datasetContext.Add(dfsDataFlowBasic);

            //Generate ingestion steps (get file to raw location)
            AddDataFlowStep(dto, df, DataActionType.ProducerS3Drop);

            AddDataFlowStep(dto, df, DataActionType.RawStorage);

            if (dto.IsCompressed)
            {
                switch (dto.CompressionJob.CompressionType)
                {
                    case CompressionTypes.ZIP:
                        AddDataFlowStep(dto, df, DataActionType.UncompressZip);
                        break;
                    case CompressionTypes.GZIP:
                        AddDataFlowStep(dto, df, DataActionType.UncompressGzip);
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

            _jobService.CreateDropLocation(dfsDataFlowBasic);
        }

        private void MapDataFlowStepsForPull(DataFlowDto dto, DataFlow df)
        {
            dto.RetrieverJob.DataFlow = df.Id;
            _jobService.CreateAndSaveRetrieverJob(dto.RetrieverJob);

            //Generate ingestion steps (get file to raw location)
            AddDataFlowStep(dto, df, DataActionType.ProducerS3Drop);

            AddDataFlowStep(dto, df, DataActionType.RawStorage);

            if (dto.IsCompressed)
            {
                switch (dto.CompressionJob.CompressionType)
                {
                    case CompressionTypes.ZIP:
                        AddDataFlowStep(dto, df, DataActionType.UncompressZip);
                        break;
                    case CompressionTypes.GZIP:
                        AddDataFlowStep(dto, df, DataActionType.UncompressGzip);
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

        private SchemaMap MapToSchemaMap(SchemaMapDto dto, DataFlowStep step)
        {
            SchemaMap map =  new SchemaMap()
            {
                DataFlowStepId = step,
                MappedSchema = _datasetContext.FileSchema.Where(w => w.SchemaId == dto.SchemaId).FirstOrDefault(),
                Dataset = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == dto.SchemaId).Select(s => s.ParentDataset).FirstOrDefault(),
                SearchCriteria = dto.SearchCriteria
            };

            _datasetContext.Add(map);

            return map;
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
            dto.SaidKeyCode = df.SaidKeyCode;
            dto.Name = df.Name;
            dto.CreateDTM = df.CreatedDTM;
            dto.CreatedBy = df.CreatedBy;
            dto.FlowStorageCode = df.FlowStorageCode;
            dto.MappedSchema = GetMappedFileSchema(df.Id);
            dto.AssociatedJobs = GetExternalRetrieverJobs(df.Id);
            dto.ObjectStatus = df.ObjectStatus;
            dto.DeleteIssuer = df.DeleteIssuer;
            dto.DeleteIssueDTM = df.DeleteIssueDTM;

            List<SchemaMapDto> scmMapDtoList = new List<SchemaMapDto>();
            foreach (DataFlowStep step in df.Steps.Where(w => w.SchemaMappings != null && w.SchemaMappings.Any()))
            {
                foreach(SchemaMap map in step.SchemaMappings)
                {
                    scmMapDtoList.Add(map.ToDto());
                }
            }
        }

        private List<int> GetMappedFileSchema(int dataflowId)
        {
            return _datasetContext.SchemaMap.Where(w => w.DataFlowStepId.DataFlow.Id == dataflowId).Select(s => s.MappedSchema.SchemaId).ToList();
        }

        private List<int> GetExternalRetrieverJobs(int dataflowId)
        {
            List<int> rjList = _datasetContext.RetrieverJob.Where(w => w.DataFlow.Id == dataflowId && !w.IsGeneric).Select(s => s.Id).ToList();
            return rjList;
        }

        private void MapToDetailDto(DataFlow flow, DataFlowDetailDto dto)
        {
            MapToDto(flow, dto);

            List<DataFlowStepDto> stepDtoList = new List<DataFlowStepDto>();
            MapToDtoList(flow.Steps.ToList(), stepDtoList);

            dto.steps = stepDtoList;

        }

        private void MapToDetailDto(SchemaMap map, SchemaMapDetailDto dto)
        {
            MapToDto(map, dto);

            dto.SchemaName = map.MappedSchema.Name;
            dto.DatasetName = map.Dataset.DatasetName;
        }

        private void MapToDto(DataFlowStep step, DataFlowStepDto dto)
        {
            dto.Id = step.Id;
            dto.ActionId = step.Action.Id;
            dto.DataActionType = step.DataAction_Type_Id;
            dto.ExeuctionOrder = step.ExeuctionOrder;
            dto.ActionName = step.Action.Name;
            dto.ActionDescription = step.Action.Description;
            dto.TriggerKey = step.TriggerKey;
            dto.TriggerBucket = step.Action.TargetStorageBucket;
            dto.TargetPrefix = step.TargetPrefix;
            dto.DataFlowId = step.DataFlow.Id;
        }

        private void MapToDto(SchemaMap map, SchemaMapDto dto)
        {
            dto.Id = map.Id;
            dto.DatasetId = map.Dataset.DatasetId;
            dto.IsDeleted = false;
            dto.SchemaId = map.MappedSchema.SchemaId;
            dto.SearchCriteria = map.SearchCriteria;
            dto.StepId = map.DataFlowStepId.Id;
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
                    break;
                case DataActionType.ProducerS3Drop:
                    action = _datasetContext.ProducerS3DropAction.FirstOrDefault();
                    break;
                case DataActionType.RawStorage:
                    action = _datasetContext.RawStorageAction.FirstOrDefault();
                    break;
                case DataActionType.QueryStorage:
                    action = _datasetContext.QueryStorageAction.FirstOrDefault();
                    break;
                case DataActionType.ConvertParquet:
                    action = _datasetContext.ConvertToParquetAction.FirstOrDefault();
                    break;
                case DataActionType.UncompressZip:
                    action = _datasetContext.UncompressZipAction.FirstOrDefault();
                    break;
                case DataActionType.GoogleApi:
                    action = _datasetContext.GoogleApiAction.FirstOrDefault();
                    break;
                case DataActionType.ClaimIq:
                    action = _datasetContext.ClaimIQAction.FirstOrDefault();
                    break;
                case DataActionType.UncompressGzip:
                    action = _datasetContext.UncompressGzipAction.FirstOrDefault();
                    break;
                case DataActionType.FixedWidth:
                    action = _datasetContext.FixedWidthAction.FirstOrDefault();
                    break;
                case DataActionType.XML:
                    action = _datasetContext.XMLAction.FirstOrDefault();
                    break;
                case DataActionType.JsonFlattening:
                    action = _datasetContext.JsonFlatteningAction.FirstOrDefault();
                    break;
                case DataActionType.SchemaLoad:
                    action = _datasetContext.SchemaLoadAction.FirstOrDefault();
                    DataFlowStep schemaLoadStep = MapToDataFlowStep(df, action, actionType);
                    List<SchemaMap> schemaMapList = new List<SchemaMap>();
                    foreach (SchemaMapDto mapDto in dto.SchemaMap.Where(w => !w.IsDeleted))
                    {
                        schemaMapList.Add(MapToSchemaMap(mapDto, schemaLoadStep));
                    }
                    schemaLoadStep.SchemaMappings = schemaMapList;
                    return schemaLoadStep;
                case DataActionType.SchemaMap:
                    action = _datasetContext.SchemaMapAction.FirstOrDefault();
                    DataFlowStep schemaMapStep = MapToDataFlowStep(df, action, actionType);
                    foreach (SchemaMapDto mapDto in dto.SchemaMap.Where(w => !w.IsDeleted))
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
            else if (step.DataAction_Type_Id == DataActionType.ProducerS3Drop)
            {
                step.TriggerKey = $"droplocation/data/{step.DataFlow.SaidKeyCode}/{step.DataFlow.FlowStorageCode}/";
            }
            else
            {
                step.TriggerKey = $"{GlobalConstants.DataFlowTargetPrefixes.TEMP_FILE_PREFIX}{step.Action.TargetStoragePrefix}{Configuration.Config.GetHostSetting("S3DataPrefix")}{step.DataFlow.FlowStorageCode}/";
            }            
        }

        private void SetTargetPrefix(DataFlowStep step)
        {
            switch (step.DataAction_Type_Id)
            {
                case DataActionType.None:
                    break;
                //These send output to schema aware storage
                case DataActionType.QueryStorage:
                case DataActionType.ConvertParquet:
                    string schemaStorageCode = GetSchemaStorageCodeForDataFlow(step.DataFlow.Id);
                    step.TargetPrefix = step.Action.TargetStoragePrefix + $"{Configuration.Config.GetHostSetting("S3DataPrefix")}{schemaStorageCode}/";
                    break;
                //These sent output a step specific location along with down stream dependent steps
                case DataActionType.RawStorage:
                    step.TargetPrefix = step.Action.TargetStoragePrefix + $"{Configuration.Config.GetHostSetting("S3DataPrefix")}{step.DataFlow.FlowStorageCode}/";
                    break;
                //These only send output to down stream dependent steps
                case DataActionType.SchemaLoad:
                case DataActionType.UncompressZip:
                case DataActionType.UncompressGzip:
                case DataActionType.SchemaMap:
                case DataActionType.S3Drop:
                case DataActionType.ProducerS3Drop:
                case DataActionType.FixedWidth:
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
                FlowStorageCode = _datasetContext.GetNextDataFlowStorageCDE(),
                DeleteIssueDTM = DateTime.MaxValue,
                ObjectStatus = Core.GlobalEnums.ObjectStatusEnum.Active
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
            List<DataSource> srcList = _datasetContext.DataSources.ToList();
            RetrieverJob dfsDataFlowBasic = _jobService.InstantiateJobsForCreation(df, srcList.First(w => w.SourceType == GlobalConstants.DataSoureDiscriminator.DEFAULT_DATAFLOW_DFS_DROP_LOCATION));
            _datasetContext.Add(dfsDataFlowBasic);

            //Generate ingestion steps (get file to raw location)
            AddDataFlowStep(dto, df, DataActionType.S3Drop);
            AddDataFlowStep(dto, df, DataActionType.RawStorage);

            //Generate preprocessing for file types (i.e. fixedwidth, csv, json, etc...)
            MapPreProcessingSteps(scm, dto, df);

            //Generate DSC registering step
            AddDataFlowStep(dto, df, DataActionType.SchemaLoad);
            AddDataFlowStep(dto, df, DataActionType.QueryStorage);

            ////Generate consumption layer steps
            AddDataFlowStep(dto, df, DataActionType.ConvertParquet);

        }

        private void MapPreProcessingSteps(FileSchema scm, DataFlowDto dto, DataFlow df)
        {
            if (scm.Extension.Name.ToUpper() == "FIXEDWIDTH")
            {
                AddDataFlowStep(dto, df, DataActionType.FixedWidth);
            }
            else if (scm.Extension.Name.ToUpper() == "XML")
            {
                AddDataFlowStep(dto, df, DataActionType.XML);
            }
            else if (scm.Extension.Name.ToUpper() == "JSON" && !String.IsNullOrWhiteSpace(scm.SchemaRootPath))
            {
                AddDataFlowStep(dto, df, DataActionType.JsonFlattening);
            }
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
