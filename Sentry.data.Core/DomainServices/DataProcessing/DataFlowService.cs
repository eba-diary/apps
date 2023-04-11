using Hangfire;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.DTO.Security;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.Jira;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces.QuartermasterRestClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFlowService : IDataFlowService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;
        private readonly IJobService _jobService;
        private readonly ISecurityService _securityService;
        private readonly IQuartermasterService _quartermasterService;
        private readonly IDataFeatures _dataFeatures;
        private readonly IBackgroundJobClient _hangfireBackgroundJobClient;
        private readonly IEmailService _emailService;
        private readonly IKafkaConnectorService _connectorService;

        public DataFlowService(
                IDatasetContext datasetContext, 
                IUserService userService, 
                IJobService jobService,
                ISecurityService securityService, 
                IQuartermasterService quartermasterService, 
                IDataFeatures dataFeatures, 
                IBackgroundJobClient backgroundJobClient,
                IEmailService emailService,
                IKafkaConnectorService connectorService)
        {
            _datasetContext = datasetContext;
            _userService = userService;
            _jobService = jobService;
            _securityService = securityService;
            _quartermasterService = quartermasterService;
            _dataFeatures = dataFeatures;
            _hangfireBackgroundJobClient = backgroundJobClient;
            _emailService = emailService;
            _connectorService = connectorService;
        }

        public List<DataFlowDto> ListDataFlows()
        {
            List<DataFlow> dfList = _datasetContext.DataFlow.ToList();
            List<DataFlowDto> dtoList = new List<DataFlowDto>();
            MapToDtoList(dfList, dtoList);
            return dtoList;
        }

        public DataFlowDto GetDataFlowDto(int id)
        {
            DataFlow df = _datasetContext.GetById<DataFlow>(id);
            DataFlowDto dto = new DataFlowDto();
            MapToDto(df, dto);
            return dto;
        }

        public DataFlowDetailDto GetDataFlowDetailDto(int id)
        {
            DataFlow df = _datasetContext.GetById<DataFlow>(id);

            if (df == null)
            {
                throw new DataFlowNotFound();
            }

            DataFlowDetailDto dto = new DataFlowDetailDto();
            MapToDetailDto(df, dto);
            return dto;
        }

        private List<DataFlowDetailDto> GetDataFlowDetailDto(Expression<Func<DataFlow, bool>> expression)
        {
            List<DataFlow> dfList = null;

            dfList = _datasetContext.DataFlow.Where(expression)
                .Where(x => x.ObjectStatus == ObjectStatusEnum.Active || x.ObjectStatus == ObjectStatusEnum.Disabled)
                .OrderByDescending(x => x.Id).ToList();

            List<DataFlowDetailDto> dtoList = new List<DataFlowDetailDto>();
            MapToDetailDtoList(dfList, dtoList);
            return dtoList;
        }

        public List<DataFlowDetailDto> GetDataFlowDetailDtoByDatasetId(int datasetId)
        {
            List<DataFlowDetailDto> dtoList = GetDataFlowDetailDto(w => w.DatasetId == datasetId);
            return dtoList;
        }

        public List<DataFlowDetailDto> GetDataFlowDetailDtoBySchemaId(int schemaId)
        {
            List<DataFlowDetailDto> dtoList = GetDataFlowDetailDto(w => w.SchemaId == schemaId);
            return dtoList;
        }

        public List<DataFlowDetailDto> GetDataFlowDetailDtoByStorageCode(string storageCode)
        {
            List<DataFlowDetailDto> dtoList = GetDataFlowDetailDto(w => w.FlowStorageCode == storageCode);
            return dtoList;
        }

        public List<DataFlowDetailDto> GetDataFlowDetailDtoByTopicName(string topicName)
        {
            //NOTE: AVOID ISSUES BY DOING TOUPPER, ALSO HAD ISSUES TO DB VALUES OF NULL AND TOUPPER SO PERFORM NULL CHECK FIRST
            List<DataFlowDetailDto> dtoList = GetDataFlowDetailDto(w => w.TopicName != null && w.TopicName.ToUpper() == topicName.ToUpper());
            return dtoList;
        }

        public List<DataFlowStepDto> GetDataFlowStepDtoByTrigger(string key)
        {
            List<DataFlowStep> dfsList = _datasetContext.DataFlowStep.Where(w => w.TriggerKey == key).ToList();
            List<DataFlowStepDto> dtoList = new List<DataFlowStepDto>();
            MapToDtoList(dfsList, dtoList);
            return dtoList;
        }

        public RetrieverJobDto GetAssociatedRetrieverJobDto(int id)
        {
            RetrieverJob job = _datasetContext.RetrieverJob.Where(w => w.DataFlow.Id == id).First();
            RetrieverJobDto dto = job.ToDto();
            return dto;
        }

        /// <summary>
        /// Will enqueue a hangfire job, for each id in idList,
        ///   that will run on hangfire background server and peform
        ///   the dataflow delete.
        /// </summary>
        /// <param name="idList"></param>
        /// <remarks> This will serves an Admin only funtionlaity within DataFlow API </remarks>
        public bool Delete_Queue(List<int> idList, string userId, bool logicalDelete)
        {
            string methodName = $"{nameof(DataFlowService).ToLower()}_{nameof(Delete).ToLower()}";
            Logger.Info($"{methodName} Method Start");

            foreach(int id in idList)
            {
                _hangfireBackgroundJobClient.Enqueue<DataFlowService>(x => x.Delete(id, userId, true));                
            }

            Logger.Info($"{methodName} Method End");
            return true;
        }

        /// <summary>
        /// Only used to facilitate hangfire delete call
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="logicalDelete"></param>
        public void Delete(int id, string userId, bool logicalDelete)
        {
            IApplicationUser user = _userService.GetByAssociateId(userId);            
            bool isSuccessfull = Delete(id, user, logicalDelete);
            if (!isSuccessfull)
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// For the list of dataflow ids provided, this will set ObjectStatus appropriately based on logicDelete flag.
        /// In addition,
        ///   will find any retrieverjobs, associated with specified dataflow, and 
        ///   set its ObjectStatus = Deleted.
        /// </summary>
        /// <param name="idList"></param>
        /// <param name="user"></param>
        /// <param name="logicalDelete"></param>
        /// <remarks>logicalDelete = true sets objectstatus to Pending_Delete. 
        /// logicalDelete = false sets objectstatus to Deleted.</remarks>
        public bool Delete(List<int> idList, IApplicationUser user, bool logicalDelete)
        {
            bool allDeletesSuccessfull = true;
            foreach (int id in idList)
            {
                bool isSuccessful = Delete(id, user, logicalDelete);
                if (!isSuccessful)
                {
                    allDeletesSuccessfull = isSuccessful;
                }
            }
            return allDeletesSuccessfull;
        }

        /// <summary>
        /// This will set ObjectStatus = Deleted for specified dataflow.  In addition,
        ///   will find any retrieverjobs, associated with specified dataflow, and 
        ///   set its ObjectStatus = Deleted.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <param name="logicalDelete"></param>
        /// <remarks>
        /// This method can be triggered by Hangfire.  
        /// Added the AutomaticRetry attribute to ensure retries do not occur for this method.
        /// https://docs.hangfire.io/en/latest/background-processing/dealing-with-exceptions.html
        /// </remarks>
        [AutomaticRetry(Attempts = 0)]
        public bool Delete(int id, IApplicationUser user, bool logicalDelete)
        {
            string methodName = $"{nameof(DataFlowService).ToLower()}_{nameof(Delete).ToLower()}";
            Logger.Debug($"{methodName} Method Start");
            Logger.Info($"{methodName} - dataflowid:{id}");

            bool returnResult = true;

            //Find DataFlow
            DataFlow flow = _datasetContext.GetById<DataFlow>(id);

            bool PerformDeleteLogic = true;
            if (flow == null)
            {
                Logger.Debug($"{methodName} DataFlow not found - dataflowid:{id}");
                throw new DataFlowNotFound();
            }

            //Determine if flow is already deleted
            if (logicalDelete && (flow.ObjectStatus == ObjectStatusEnum.Pending_Delete || flow.ObjectStatus == ObjectStatusEnum.Deleted))
            {
                PerformDeleteLogic = false;
            }
            else if (!logicalDelete && flow.ObjectStatus == GlobalEnums.ObjectStatusEnum.Deleted)
            {
                PerformDeleteLogic = false;
            }

            if (!PerformDeleteLogic)
            {
                Logger.Info($"{methodName} Object already has status {flow.ObjectStatus}");
                Logger.Info($"{methodName} Method End");
                return returnResult;
            }

            if (logicalDelete)
            {
                //Mark dataflow deleted
                flow.ObjectStatus = GlobalEnums.ObjectStatusEnum.Pending_Delete;
                flow.DeleteIssuer = flow.DeleteIssuer ?? user.AssociateId.ToString();

                //Only comparing date since the milliseconds percision are different, therefore, never evaluates true
                //  https://stackoverflow.com/a/44324883
                if (DateTime.MaxValue.Date == flow.DeleteIssueDTM.Date)
                {
                    flow.DeleteIssueDTM = DateTime.Now;
                }

                //Delete associated retriever jobs
                List<int> jobList = GetRetrieverJobsByDataFlowId(id);
                if (jobList.Any())
                {
                    _jobService.Delete(jobList, user, true);
                }

            }
            else
            {
                //Mark dataflow deleted
                flow.ObjectStatus = GlobalEnums.ObjectStatusEnum.Deleted;
                flow.DeleteIssuer = flow.DeleteIssuer ?? user.AssociateId.ToString();

                //Only comparing date since the milliseconds percision are different, therefore, never evaluates true
                //  https://stackoverflow.com/a/44324883
                if (DateTime.MaxValue.Date == flow.DeleteIssueDTM.Date)
                {
                    flow.DeleteIssueDTM = DateTime.Now;
                }

                //Delete associated retriever jobs
                List<int> jobList = _datasetContext.RetrieverJob.Where(w => w.DataFlow.Id == id).Select(s => s.Id).ToList();
                _jobService.Delete(jobList, user, false);
            }

            Logger.Debug($"{methodName} Method End");
            return returnResult;
        }

        private List<int> GetRetrieverJobsByDataFlowId(int id)
        {
            List<int> jobIdList = new List<int>();
            jobIdList.AddRange(_datasetContext.RetrieverJob.Where(w => w.DataFlow.Id == id).Select(s => s.Id));
            return jobIdList;
        }

        public async Task<DataFlowDto> AddDataFlowAsync(DataFlowDto dto)
        {
            DataFlowDto resultDto = await AddDataFlowAsync_Internal(dto);

            if (_dataFeatures.CLA3718_Authorization.GetValue())
            {
                // Create a Hangfire job that will setup the default security groups for this new dataset
                _securityService.EnqueueCreateDefaultSecurityForDataFlow(resultDto.Id);
            }

            return resultDto;
        }

        private Task<DataFlowDto> AddDataFlowAsync_Internal(DataFlowDto dto)
        {
            dto.CreatedBy = _userService.GetCurrentUser().AssociateId;

            DataFlow dataFlow = CreateAndSaveDataFlow(dto);

            DataFlowDto resultDto = new DataFlowDto();
            MapToDto(dataFlow, resultDto);

            return Task.FromResult(resultDto);
        }

        public DataFlow Create(DataFlowDto dto)
        {
            string methodName = $"{nameof(DataFlowService).ToLower()}_{nameof(Create).ToLower()}";
            Logger.Info($"{methodName} Method Start");            

            DataFlow newDataFlow;
            try
            {
                ValidateDataFlowDtoForCreate(dto);

                newDataFlow = CreateDataflow_Internal(dto);
            }
            catch (Exception ex)
            {
                Logger.Error($"{methodName} - Failed to create dataflow", ex);
                throw;
            }

            Logger.Info($"{methodName} Method End");
            return newDataFlow;
        }

        private DataFlow CreateDataflow_Internal(DataFlowDto dto)
        {
            DataFlow newDataFlow = MapToDataFlow(dto);
            MapDataFlowSteps(dto, newDataFlow);

            return newDataFlow;
        }

        /// <summary>
        /// Create all dataflow external dependiences.
        /// </summary>
        /// <param name="dataFlow"></param>
        public void CreateExternalDependencies(int dataFlowId)
        {
            DataFlow dataFlow = _datasetContext.GetById<DataFlow>(dataFlowId);

            if (_dataFeatures.CLA3718_Authorization.GetValue())
            {
                // Create a Hangfire job that will setup the default security groups for this new dataset
                _securityService.EnqueueCreateDefaultSecurityForDataFlow(dataFlow.Id);
            }
            //Create S3 Sink Connectors
            CreateS3SinkConnector(dataFlow);
            //Create DFS Drop locations
            CreateDataFlowDfsDropLocation(dataFlow);
        }

        public int CreateDataFlow(DataFlowDto dto)
        {
            ValidateDataFlowDtoForCreate(dto);

            try
            {
                DataFlow df = CreateAndSaveDataFlow(dto);

                if (_dataFeatures.CLA3718_Authorization.GetValue())
                {
                    // Create a Hangfire job that will setup the default security groups for this new dataset
                    _securityService.EnqueueCreateDefaultSecurityForDataFlow(df.Id);
                }

                return df.Id;
            }
            catch (Exception ex)
            {
                Logger.Error("dataflowservice-createandsavedataflow failed to save dataflow", ex);
                throw;
            }
        }

        public async Task<DataFlowDto> UpdateDataFlowAsync(DataFlowDto dto, DataFlow dataFlow)
        {
            //immutable properties that must keep original value
            dto.Id = dataFlow.Id;
            dto.SaidKeyCode = dataFlow.SaidKeyCode;
            dto.NamedEnvironment = dataFlow.NamedEnvironment;
            dto.NamedEnvironmentType = dataFlow.NamedEnvironmentType;
            dto.ObjectStatus = dataFlow.ObjectStatus;
            dto.DatasetId = dataFlow.DatasetId;
            dto.Name = dataFlow.Name;
            dto.IsSecured = dataFlow.IsSecured;
            dto.SchemaMap = new List<SchemaMapDto>
            {
                new SchemaMapDto { DatasetId = dataFlow.DatasetId, SchemaId = dataFlow.SchemaId }
            };

            DataFlowDto resultDto = new DataFlowDto();

            //only update data flow if any of the data flow properties are different or data flow steps need to be updated
            if (DataFlowHasUpdates(dto, dataFlow) || dto.DataFlowStepUpdateRequired)
            {
                //make sure dto properties are set with updated properties
                //in some cases a null value means not to update the property
                SetUnchangedProperties(dto, dataFlow);

                //delete and create new dataflow
                Delete(dto.Id, _userService.GetCurrentUser(), false);
                resultDto = await AddDataFlowAsync_Internal(dto);
            }
            else
            {
                //return data flow as is
                MapToDto(dataFlow, resultDto);
            }

            return resultDto;
        }

        private bool DataFlowHasUpdates(DataFlowDto dto, DataFlow dataFlow)
        {
            //broken out into individual IF statement for readability
            if (dto.IngestionType > 0)
            {
                //ingestion type is populated and different
                if (dto.IngestionType != dataFlow.IngestionType)
                {
                    return true;
                }
                //ingestion type is topic and topic name is different
                else if (dto.IngestionType == (int)IngestionType.Topic && dto.TopicName != dataFlow.TopicName)
                {
                    return true;
                }
            }

            //contact id is populated and different
            if (!string.IsNullOrWhiteSpace(dto.PrimaryContactId) && dto.PrimaryContactId != dataFlow.PrimaryContactId)
            {
                return true;
            }

            //is compressed is different
            if (dto.IsCompressed != dataFlow.IsDecompressionRequired)
            {
                return true;
            }
            //different compression option
            else if (dto.IsCompressed && dto.CompressionType != dataFlow.CompressionType)
            {
                return true;
            }

            //is preprocessing different
            if (dto.IsPreProcessingRequired != dataFlow.IsPreProcessingRequired)
            {
                return true;
            }
            //different preprocessing option
            else if (dto.IsPreProcessingRequired && dto.PreProcessingOption != dataFlow.PreProcessingOption)
            {
                return true;
            }

            return false;
        }

        private void SetUnchangedProperties(DataFlowDto dto, DataFlow dataFlow)
        {
            //keep original ingestion type because no new value was provided
            if (dto.IngestionType == 0)
            {
                dto.IngestionType = dataFlow.IngestionType;
            }

            //keep original contact because no new value was provided
            if (string.IsNullOrWhiteSpace(dto.PrimaryContactId) && dto.PrimaryContactId != dataFlow.PrimaryContactId)
            {
                dto.PrimaryContactId = dataFlow.PrimaryContactId;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dfDto"></param>
        /// <param name="deleteOriginal"></param>
        /// <returns></returns>
        public int UpdateDataFlow(DataFlowDto dfDto)
        {
            /*
             *  Logically delete the existing dataflow
             *    This will take care of deleting any existing 
             *    retriever jobs and removing them from hangfire
             *  WallEService will eventually set the objects
             *    to a deleted status after a set period of time    
             */
            Delete(dfDto.Id, _userService.GetCurrentUser(), false);

            /*
             *  Create new Dataflow
             *  - The incoming dto will have flowstoragecode and will
             *     be used by new dataflow as well  
            */
            DataFlow newDataFlow = CreateAndSaveDataFlow(dfDto);

            return newDataFlow.Id;
        }

        public void EnableOrDisableDataFlow(int dataFlowId, ObjectStatusEnum status)
        {
            if (status != ObjectStatusEnum.Active && status != ObjectStatusEnum.Disabled)
            {
                throw new ArgumentException("Active or Disabled Object Status only allowed",nameof(status));
            }

            try
            {
                //Find DataFlow
                DataFlow flow = _datasetContext.GetById<DataFlow>(dataFlowId);
                flow.ObjectStatus = status;
                _datasetContext.SaveChanges();
            }
            catch(Exception ex) 
            {
                Logger.Error($"{nameof(DataFlowService)}.{nameof(EnableOrDisableDataFlow)} failed", ex);
                throw;
            }
        }


        public void CreateDataFlowForSchema(FileSchema scm)
        {
            DataFlow df = MapToDataFlow(scm);

            MapDataFlowStepsForFileSchema(scm, df);
            
            if (df.ShouldCreateDFSDropLocations(_dataFeatures))
            {
                _jobService.CreateDropLocation(_datasetContext.RetrieverJob.FirstOrDefault(w => w.DataFlow == df));
            }
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
            if (scm == null)
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
        
        /*
         *  @param - User is required to pass Dataflowstep id to being processing from
         *  @return - DataFlowdto object of the stepId
         */
        public DataFlowDto GetDataFlowByStepId(int stepId)
        {
            if (stepId == 0)
            {
                throw new ArgumentNullException("stepId", "DataFlowStep is required");
            }

            // finding the dataflowstep with the associated stepId
            DataFlowStep step = _datasetContext.DataFlowStep.Where(w => w.Id == stepId).FirstOrDefault();
            
            // retrieve the dataflow object from dataflowstep
            DataFlow df = step.DataFlow;

            DataFlowDto dataFlowDto = new DataFlowDto();
            MapToDto(df, dataFlowDto);

            return dataFlowDto;
        }

        /*
         *  Helper method for getting a dataFlotDto object from a DataflowStepid
         *  @param - User is required to pass Dataflowstep id to being processing from
         *  @return - DataFlowdto object of the stepId
         */
        public DataFlowDto GetDataFlowDtoByStepId(int stepId)
        {
            if (stepId == 0) // making sure stepId is a valid value
            {
                throw new ArgumentNullException("stepId", "DataFlowStep is required");
            }

            DataFlowStep dataFlowStep = _datasetContext.DataFlowStep.Where(w => w.Id == stepId).FirstOrDefault();

            // the case when the stepId cannot be found
            if(dataFlowStep == null)
            {
                throw new DataFlowStepNotFound("stepId not found");
            }

            // finding the dataflowstep with the associated stepId and retrieving the dataflow object from dataflowstep
            DataFlow df = dataFlowStep.DataFlow;
            
            // creating a new blank DataFlowDto object
            DataFlowDto dataFlowDto = new DataFlowDto();

            // creating a DataFlowDto from the DataFlow
            MapToDto(df, dataFlowDto);
            
            return dataFlowDto;
        }

        /*
         *  Helper method for getting a datasetFileId into a schema id
         *  @param - int datasetFileId
         *  @return - int schemaId
         */
        public int GetSchemaIdFromDatasetFileId(int datasetFileId)
        {
            if(datasetFileId == 0)
            {
                throw new ArgumentNullException("datasetFileId", "DatasetFileId is required attribute");
            }
            
            DatasetFile datasetFile = _datasetContext.DatasetFileStatusActive.Where(w => w.DatasetFileId == datasetFileId).FirstOrDefault();

            // the case when the datasetFileId is not found
            if(datasetFile == null)
            {
                throw new DataFileNotFoundException("DatasetFileId was not found");
            }

            // finds the DatasetFile object that is associated with the datasetFileId passed into the method and getting schema id 
            int schemaId = datasetFile.Schema.SchemaId;


            return schemaId; // returns the schema id of the associated datasetFileId
        }
        
        /*
         * Validating that all datasetFileIds correspond to the stepId
         * @return true->passed validation, false->failed validation   determines whether reprocessing should be performed or not
         */
        public bool ValidateStepIdAndDatasetFileIds(int stepId, List<int> datasetFileIds)
        {
            bool indicator = true;

            // creates a dataFlowDto object from the stepId
            DataFlowDto currentDataFlowDto = new DataFlowDto();

            try
            {
                currentDataFlowDto = GetDataFlowDtoByStepId(stepId);
            } catch (DataFlowStepNotFound)
            {
                 return false;
            }
            
            try
            {
                // traversing through the list of datasetFileIds
                foreach (int datasetFileId in datasetFileIds)
                {
                    // compares the schemaIds from the DataFlowDto and the datasetFileId seeing if they are not equal
                    if (currentDataFlowDto.SchemaId != GetSchemaIdFromDatasetFileId(datasetFileId))
                    {
                        // in the case that the schemaId are not equal to one another --> return false
                        indicator = false;
                        break;
                    }


                }
            } catch (DataFileNotFoundException)
            {
                indicator = false;
            }
            
            
            
            // if both schemaIds are equal for all datasetFileIds then this method will return true, false otherwise
            return indicator;
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
            steps = _datasetContext.DataFlowStep.Where(w => w.SourceDependencyPrefix == step.TriggerKey && w.SourceDependencyBucket == step.TriggerBucket).ToList();

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

        private List<int> GetProducerFlowsToBeDeletedBySchemaId(int schemaId)
        {
            Logger.Info($"{nameof(DataFlowService).ToLower()}_{nameof(GetProducerFlowsToBeDeletedBySchemaId).ToLower()} Method Start");
            List<int> producerFlowIdList = new List<int>();

            /* Get producer flow Ids which map to schema that are active*/
            List<Tuple<int, int>> producerSchemaMapIds = _datasetContext.SchemaMap.Where(w => 
                                            w.MappedSchema.SchemaId == schemaId
                                            && !w.DataFlowStepId.DataFlow.Name.Contains("FileSchemaFlow")
                                            && w.DataFlowStepId.DataFlow.ObjectStatus != GlobalEnums.ObjectStatusEnum.Deleted
                                            ).Select(s => new Tuple<int, int>(s.DataFlowStepId.Id, s.DataFlowStepId.DataFlow.Id)).ToList();

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

            Logger.Info($"{nameof(DataFlowService).ToLower()}_{nameof(GetProducerFlowsToBeDeletedBySchemaId).ToLower()} Method End");
            return producerFlowIdList;
        }



        public List<SchemaMapDetailDto> GetMappedSchemaByDataFlow(int dataflowId)
        {
            List<SchemaMap> schemaMapList = _datasetContext.DataFlowStep.Where(w => w.DataFlow.Id == dataflowId).SelectMany(s => s.SchemaMappings).ToList();
            List<SchemaMapDetailDto> dtoList = new List<SchemaMapDetailDto>();
            foreach (SchemaMap map in schemaMapList)
            {
                SchemaMapDetailDto dto = new SchemaMapDetailDto();
                MapToDetailDto(map, dto);
                dtoList.Add(dto);
            }
            return dtoList;
        }

        public async Task<ValidationException> ValidateAsync(DataFlowDto dfDto)
        {
            ValidationResults results = new ValidationResults();
            if (dfDto.Id == 0 &&_datasetContext.DataFlow.Any(w => w.Name == dfDto.Name))
            {
                results.Add(DataFlow.ValidationErrors.nameMustBeUnique, "Dataflow name is already used");
            }

            //IF THIS IS A BRAND NEW DATAFLOW AND TOPIC INGESTION TYPE IS TOPIC THEN MAKE SURE TOPIC NAME DOESN'T ALREADY EXIST
            if (    dfDto.Id == 0 
                    && dfDto.IngestionType == (int) IngestionType.Topic 
                    && _datasetContext.DataFlow.Any(w => w.TopicName == dfDto.TopicName))
            {
                results.Add(DataFlow.ValidationErrors.topicNameMustBeUnique, "Kafka Topic name is already used");
            }

            //Validate the Named Environment selection using the QuartermasterService
            results.MergeInResults(await _quartermasterService.VerifyNamedEnvironmentAsync(dfDto.SaidKeyCode, dfDto.NamedEnvironment, dfDto.NamedEnvironmentType).ConfigureAwait(false));

            return new ValidationException(results);
        }

        #region Private Methods
        internal virtual void ValidateDataFlowDtoForCreate(DataFlowDto dto)
        {
            //Verify user has permissions to create Dataflow
            UserSecurity us = _securityService.GetUserSecurity(null, _userService.GetCurrentUser());

            if (!us.CanCreateDataFlow)
            {
                throw new DataFlowUnauthorizedAccessException();
            }

            //Verify user has permissions to push data to each schema mapped to dataflow
            StringBuilder datasetsWithNoPermissions = new StringBuilder();
            foreach (SchemaMapDto scmMap in dto.SchemaMap)
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

            //Verify that the schema selected is not already connected with a different dataflow
            if (_datasetContext.DataFlow.Any(df => df.DatasetId == dto.SchemaMap.First().DatasetId &&
                                                   df.SchemaId == dto.SchemaMap.First().SchemaId &&
                                                   df.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active))
            {
                throw new SchemaInUseException($"Schema ID {dto.SchemaMap.First().SchemaId} is already associated to another DataFlow.");
            }
        }

        internal void CreateS3SinkConnector(DataFlow df)
        {
            //ONLY CreateS3SinkConnector IF FEATURE FLAG IS ON: REMOVE THIS WHOLE IF STATEMENT AFTER GOLIVE
            if (!_dataFeatures.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL.GetValue())
            {
                Logger.Info($"Method {nameof(CreateS3SinkConnector)} Check feature flag {nameof(_dataFeatures.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL)} is not turned on.  No email will be sent.");
                return;
            }

            if (df.IngestionType == (int)IngestionType.Topic)
            {
                //IF TOPIC NAME EXISTS ON ANY OTHER DATAFLOW (INCLUDING AN UPDATE ON A DATAFLOW WITH SAME TOPIC NAME AS IT ALREADY HAS) THEN DO NOT CREATE S3 SINK CONNECTOR
                //COULD CREATE SCENARIO IN CONFLUENT API SINK CONNECTOR WHERE MULTIPLE CONNECTORS COULD CONNECT TO THE SAME TOPIC SINCE OLD CONNECTORS NOT CREATED BY UI HAD DIFFERENT NAMING CONVENTIONS
                //IF YOU UPDATE AN OLD DATAFLOW, S3SINKCONNECTOR WILL BE GENERATED BASED ON TOPIC NAME AND IF EXISTING CONNECTOR HAS DIFFERENT NAMING CONVENTION THEN 2 CONNECTORS WILL POINT TO SAME TOPIC
                //If topic exists on any "deleted" DataFlows matching TopicName, do not create S3SinkConnector since one was already created
                if (_datasetContext.DataFlow.Any(w => w.TopicName != null && w.TopicName.ToUpper() == df.TopicName.ToUpper() && w.ObjectStatus != ObjectStatusEnum.Active))
                {
                    Logger.Info($"Method {nameof(CreateS3SinkConnector)}: {nameof(_connectorService.CreateS3SinkConnectorAsync)} not executed because TopicName already exists on this Dataflow or others.");
                    return;
                }

                ConnectorCreateRequestDto requestDto = CreateConnectorRequestDto(df);
                //NOTE: CreateS3SinkConnectorAsync is Async but we are CHOOSING to call CALL SYNCRONOUSLY WITHOUT AWAIT which releases caller BECAUSE CreateS3SinkConnectorAsync Actually returns immediately 
                Task<ConnectorCreateResponseDto> task = _connectorService.CreateS3SinkConnectorAsync(requestDto);
                ConnectorCreateResponseDto responseDto = task.Result;                                                   //USE Task.Result to essentially Syncronously wait for Result of task, here we need to know if call to S3Sink was successful or not
                _emailService.SendS3SinkConnectorRequestEmail(df, requestDto, responseDto);                              //Send email with success or failure
            }
        }

        private ConnectorCreateRequestDto CreateConnectorRequestDto(DataFlow df)
        {
            DataFlowStep step = df.Steps.FirstOrDefault(w => w.DataAction_Type_Id == DataActionType.ProducerS3Drop);

            ConnectorCreateRequestConfigDto config = new ConnectorCreateRequestConfigDto()
            {
                ConnectorClass = "io.confluent.connect.s3.S3SinkConnector",
                S3Region = "us-east-2",
                TopicsDir = "topics_2",
                FlushSize = Configuration.Config.GetHostSetting(GlobalConstants.HostSettings.CONFLUENT_CONNECTOR_FLUSH_SIZE),
                TasksMax = "1",
                Timezone = "UTC",
                Transforms = "InsertMetadata",
                Locale = "en-US",
                S3PathStyleAccessEnabled = "false",
                FormatClass = "io.confluent.connect.s3.format.json.JsonFormat",
                S3AclCanned = "bucket-owner-full-control",
                TransformsInsertMetadataPartitionField = "kafka_partition",
                ValueConverter = "org.apache.kafka.connect.json.JsonConverter",
                S3ProxyPassword = Configuration.Config.GetHostSetting(GlobalConstants.HostSettings.CONFLUENT_CONNECTOR_PASSWORD),
                KeyConverter = "org.apache.kafka.connect.converters.ByteArrayConverter",
                S3BucketName = (step != null && step.TriggerBucket != null)? step.TriggerBucket : String.Empty,  //"sentry-dlst-qual-droplocation-ae2",
                PartitionDurationMs = "86400000",
                S3ProxyUser = Configuration.Config.GetHostSetting(GlobalConstants.HostSettings.CONFLUENT_CONNECTOR_USERNAME), //"SV_DATA_S3CON_I_Q_V1",
                S3SseaName = "AES256",
                TransformsInsertMetadataOffsetField = "kafka_offset",
                FileDelim = "_",
                Topics = df.TopicName,  //"topics": "CKMT-QUAL-INTENTIONALLOGMARKERS-01",
                PartitionerClass = "io.confluent.connect.storage.partitioner.TimeBasedPartitioner",
                ValueConverterSchemasEnable = "false",
                TransformsInsertMetadataTimestampField = "kafka_timestamp",
                Name = df.S3ConnectorName,   //"name": "S3_CKMT_QUAL_INTENTIONALLOGMARKERS_01_001",
                StorageClass = "io.confluent.connect.s3.storage.S3Storage",
                RotateScheduleIntervalMs = "86400000",
                PathFormat = "YYYY/MM/dd",
                TimestampExtractor = "Record",
                TransformsInsertMetadataType = "org.apache.kafka.connect.transforms.InsertField$Value",
                S3ProxyUrl = "https://app-proxy-nonprod.sentry.com:8080"
            };

            ConnectorCreateRequestDto request = new ConnectorCreateRequestDto()
            {
                Name = df.S3ConnectorName,
                Config = config
            };

            return request;
        }

        private DataFlow CreateAndSaveDataFlow(DataFlowDto dto)
        {
            try 
            { 
                Logger.Info($"DataFlowService_CreateAndSaveDataFlow Method Start");

                DataFlow df = MapToDataFlow(dto);

                switch (dto.IngestionType)
                {
                    case (int)IngestionType.DFS_Drop:
                    case (int)IngestionType.S3_Drop:
                    case (int)IngestionType.Topic:
                        CreateDataFlowDfsRetrieverJobMetadata(df);
                        break;
                    case (int)IngestionType.DSC_Pull:
                        CreateDataFlowExternalRetrieverJobMetadata(dto, df);
                        break;
                    default:
                        break;
                }

                MapDataFlowSteps(dto, df);

                _datasetContext.SaveChanges();

                CreateS3SinkConnector(df);

                CreateDataFlowDfsDropLocation(df);

                Logger.Info($"DataFlowService_CreateAndSaveDataFlow Method End");
                return df;
            }
            catch (ValidationException)
            {
                _datasetContext.Clear();
                throw;
            }
        }

        internal DataFlow MapToDataFlow(DataFlowDto dto)
        {
            Logger.Info($"MapToDataFlow Method Start");

            DataFlow df = new DataFlow
            {
                Name = dto.Name,
                CreatedDTM = DateTime.Now,
                CreatedBy = dto.CreatedBy,
                Questionnaire = dto.DFQuestionnaire,
                SaidKeyCode = dto.SaidKeyCode,
                ObjectStatus = dto.ObjectStatus,    //in case they are changing an existing dataset that is disabled, keep status
                DeleteIssuer = dto.DeleteIssuer,
                DeleteIssueDTM = DateTime.MaxValue,
                IngestionType = dto.IngestionType,
                IsDecompressionRequired = dto.IsCompressed,
                IsBackFillRequired = dto.IsBackFillRequired,
                CompressionType = dto.CompressionType,
                IsPreProcessingRequired = dto.IsPreProcessingRequired,
                PreProcessingOption = (int)dto.PreProcessingOption,
                NamedEnvironment = dto.NamedEnvironment,
                NamedEnvironmentType = dto.NamedEnvironmentType,
                PrimaryContactId = dto.PrimaryContactId,
                IsSecured = dto.IsSecured,
                DatasetId = dto.SchemaMap.First().DatasetId,
                SchemaId = dto.SchemaMap.First().SchemaId,

                //ONLY SET IF IngestionType.Topic
                TopicName = (dto.IngestionType == (int)IngestionType.Topic) ? dto.TopicName : null,
                S3ConnectorName = (dto.IngestionType == (int)IngestionType.Topic) ? GetS3ConnectorName(dto) : null,
                FlowStorageCode = _datasetContext.FileSchema.Where(x => x.SchemaId == dto.SchemaMap.First().SchemaId).Select(s => s.StorageCode).FirstOrDefault(),
                //All dataflows get a Security entry regardless
                //  this allows security process for internally managed permissions
                //  (i.e. CanManageDataflow)
                Security = (dto.Id == 0)
                            ? new Security(GlobalConstants.SecurableEntityName.DATAFLOW) { CreatedById = _userService.GetCurrentUser().AssociateId }
                            : _datasetContext.GetById<DataFlow>(dto.Id).Security
            };

            _datasetContext.Add(df);

            Logger.Info($"MapToDataFlow Method End");
            return df;
        }

        //GENERATE S3ConnectorName
        private string GetS3ConnectorName(DataFlowDto dto)
        {
            string cleansedTopicName = dto.TopicName.Replace("-", "_").ToUpper();
            return $"S3_{cleansedTopicName}_001";
        }

        /// <summary>
        /// Return AD group which grants CanManageDataflow permissions to dataflow.
        /// </summary>
        /// <param name="dataflowId"></param>
        /// <returns></returns>
        public string GetSecurityGroup(int dataflowId)
        {
            var datasetId = _datasetContext.GetById<DataFlow>(dataflowId).DatasetId;
            Dataset ds = _datasetContext.GetById<Dataset>(datasetId);

            if (ds == null)
            {
                return null;
            }

            var securityGroups = _securityService.GetDefaultSecurityGroupDtos(ds);

            var group = securityGroups.FirstOrDefault(w => !w.IsAssetLevelGroup() && w.GroupType == AdSecurityGroupType.Prdcr).GetGroupName();

            return group;
        }

        private void CreateDataFlowDfsRetrieverJobMetadata(DataFlow df)
        {
            string methodName = $"{nameof(DataFlowService).ToLower()}_{nameof(CreateDataFlowDfsRetrieverJobMetadata).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            //This type of dataflow does not need to worry about retrieving data from external sources
            // Data will be pushed by user to S3 and\or DFS drop locations

            Logger.Debug($"Is DataFlow Null:{df == null}");

            if (df.ShouldCreateDFSDropLocations(_dataFeatures))
            {
                DataSource dfsDataSource;
                if (string.IsNullOrEmpty(_dataFeatures.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()))
                {
                    NamedEnvironmentType datasetEnvironmentType = _datasetContext.Datasets.Where(x => x.DatasetId == df.DatasetId).Select(x => x.NamedEnvironmentType).FirstOrDefault();
                    if (datasetEnvironmentType == NamedEnvironmentType.NonProd)
                    {
                        dfsDataSource = _datasetContext.DataSources.FirstOrDefault(x => x is DfsNonProdSource);
                    }
                    else
                    {
                        dfsDataSource = _datasetContext.DataSources.FirstOrDefault(x => x is DfsProdSource);
                    }
                }
                else
                {
                    dfsDataSource = _datasetContext.DataSources.FirstOrDefault(w => w is DfsDataFlowBasic);
                }

                _ = _jobService.CreateDfsRetrieverJob(df, dfsDataSource);
            }

            Logger.Info($"{methodName} Method End");
        }

        private void CreateDataFlowDfsDropLocation(DataFlow dataflow)
        {
            if (dataflow.ShouldCreateDFSDropLocations(_dataFeatures))
            {
                RetrieverJob job = _datasetContext.RetrieverJob.FirstOrDefault(w => w.DataFlow.Id == dataflow.Id && w.ObjectStatus == ObjectStatusEnum.Active && (w.DataSource is DfsNonProdSource || w.DataSource is DfsProdSource || w.DataSource is DfsDataFlowBasic));

                if (job != null)
                {
                    _jobService.CreateDropLocation(job);
                }
            }
        }

        private void CreateDataFlowExternalRetrieverJobMetadata(DataFlowDto dto, DataFlow df)
        {
            Logger.Info($"DataFlowService_MapDataFlowStepsForPull Method Start");

            dto.RetrieverJob.DataFlow = df.Id;
            dto.RetrieverJob.FileSchema = df.SchemaId;
            _jobService.CreateRetrieverJob(dto.RetrieverJob);

            Logger.Info($"DataFlowService_MapDataFlowStepsForPull Method End");
        }

        private void MapDataFlowSteps(DataFlowDto dto, DataFlow df)
        {
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
                switch (dto.PreProcessingOption)
                {
                    case (int)DataFlowPreProcessingTypes.googleapi:
                        AddDataFlowStep(dto, df, DataActionType.GoogleApi);
                        break;
                    case (int)DataFlowPreProcessingTypes.claimiq:
                        AddDataFlowStep(dto, df, DataActionType.ClaimIq);
                        break;
                    case (int)DataFlowPreProcessingTypes.googlebigqueryapi:
                        AddDataFlowStep(dto, df, DataActionType.GoogleBigQueryApi);
                        break;
                    case (int)DataFlowPreProcessingTypes.googlesearchconsoleapi:
                        AddDataFlowStep(dto, df, DataActionType.GoogleSearchConsoleApi);
                        break;
                    default:
                        break;
                }
            }

            FileSchema scm = _datasetContext.GetById<FileSchema>(dto.SchemaMap.First().SchemaId);

            //Generate preprocessing for file types (i.e. fixedwidth, csv, json, etc...)
            MapPreProcessingSteps(scm, dto, df);

            //Generate DSC registering step
            AddDataFlowStep(dto, df, DataActionType.SchemaLoad);
            AddDataFlowStep(dto, df, DataActionType.QueryStorage);

            //Generate consumption layer steps
            if (scm.Extension.Name == GlobalConstants.ExtensionNames.PARQUET)
            {
                AddDataFlowStep(dto, df, DataActionType.CopyToParquet);
            }
            else
            {
                AddDataFlowStep(dto, df, DataActionType.ConvertParquet);
            }
        }

        private SchemaMap MapToSchemaMap(SchemaMapDto dto, DataFlowStep step)
        {
            SchemaMap map = new SchemaMap()
            {
                DataFlowStepId = step,
                MappedSchema = _datasetContext.FileSchema.Where(w => w.SchemaId == dto.SchemaId).FirstOrDefault(),
                Dataset = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == dto.SchemaId).Select(s => s.ParentDataset).FirstOrDefault(),
                SearchCriteria = dto.SearchCriteria
            };

            _datasetContext.Add(map);

            return map;
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

        private void MapToDto(DataFlow df, DataFlowDto dto)
        {
            dto.Id = df.Id;
            dto.FlowGuid = df.FlowGuid;
            dto.SaidKeyCode = df.SaidKeyCode;
            dto.Name = df.Name;
            dto.CreateDTM = df.CreatedDTM;
            dto.CreatedBy = df.CreatedBy;
            dto.FlowStorageCode = df.FlowStorageCode;
            dto.MappedSchema = (df.SchemaId != 0) ? new List<int>() { df.SchemaId } : GetMappedFileSchema(df.Id);
            dto.AssociatedJobs = GetExternalRetrieverJobs(df.Id);
            dto.ObjectStatus = df.ObjectStatus;
            dto.DeleteIssuer = df.DeleteIssuer;
            dto.DeleteIssueDTM = df.DeleteIssueDTM;
            dto.IngestionType = df.IngestionType;
            dto.IsCompressed = df.IsDecompressionRequired;
            dto.IsBackFillRequired = df.IsBackFillRequired;
            dto.CompressionType = df.CompressionType;
            dto.IsPreProcessingRequired = df.IsPreProcessingRequired;
            dto.PreProcessingOption = df.PreProcessingOption;
            dto.NamedEnvironment = df.NamedEnvironment;
            dto.NamedEnvironmentType = df.NamedEnvironmentType;
            dto.PrimaryContactId = df.PrimaryContactId;
            dto.IsSecured = df.IsSecured;
            dto.Security = _securityService.GetUserSecurity(df, _userService.GetCurrentUser());
            dto.TopicName = df.TopicName;
            dto.S3ConnectorName = df.S3ConnectorName;

            if (dto.IsCompressed)
            {
                CompressionJobDto jobDto = new CompressionJobDto
                {
                    CompressionType = (df.CompressionType.HasValue) ? (CompressionTypes)df.CompressionType : 0
                };
                dto.CompressionJob = jobDto;
            }

            List<SchemaMapDto> scmMapDtoList = new List<SchemaMapDto>();
            foreach (DataFlowStep step in df.Steps.Where(w => w.SchemaMappings != null && w.SchemaMappings.Any()))
            {
                foreach (SchemaMap map in step.SchemaMappings)
                {
                    scmMapDtoList.Add(map.ToDto());
                }
            }
            dto.SchemaMap = scmMapDtoList;

            if (dto.IngestionType == (int)IngestionType.DSC_Pull)
            {
                dto.RetrieverJob = GetAssociatedRetrieverJobDto(dto.Id);
            }

            dto.DatasetId = (df.DatasetId != 0) ? df.DatasetId : dto.SchemaMap.FirstOrDefault().DatasetId;
            dto.SchemaId = (df.SchemaId != 0) ? df.SchemaId : dto.SchemaMap.FirstOrDefault().SchemaId;
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
            dto.TriggerBucket = step.TriggerBucket;
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

        private void MapToDtoList(List<DataFlowStep> steps, List<DataFlowStepDto> dtoList)
        {
            foreach (DataFlowStep step in steps)
            {
                DataFlowStepDto stepDto = new DataFlowStepDto();
                MapToDto(step, stepDto);
                dtoList.Add(stepDto);
            }
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

        private void MapToDetailDtoList(List<DataFlow> flows, List<DataFlowDetailDto> dtoList)
        {
            foreach (DataFlow flow in flows)
            {
                DataFlowDetailDto detailDto = new DataFlowDetailDto();
                MapToDetailDto(flow, detailDto);
                dtoList.Add(detailDto);
            }
        }

        private void AddDataFlowStep(DataFlowDto dto, DataFlow df, DataActionType actionType)
        {
            DataFlowStep step = CreateDataFlowStep(actionType, dto, df);

            if (df.Steps == null)
            {
                df.Steps = new List<DataFlowStep>();
            }

            SetTrigger(step);
            SetTarget(step);
            SetSourceDependency(step, df.Steps.OrderByDescending(o => o.ExeuctionOrder).Take(1).FirstOrDefault());

            //Set exeuction order
            step.ExeuctionOrder = df.Steps.Count + 1;

            //Add to DataFlow
            df.Steps.Add(step);
        }

        private DataFlowStep CreateDataFlowStep(DataActionType actionType, DataFlowDto dto, DataFlow df)
        {
            bool isHumanResources = false;

            //Take DatasetId and figure out if Category = HR
            Dataset ds = _datasetContext.GetById<Dataset>(dto.DatasetId);
            if (ds.DatasetCategories.Any(w => w.AbbreviatedName == "HR"))
            {
                isHumanResources = true;
            }

            NamedEnvironmentType datasetNamedEnviornmentType = ds.NamedEnvironmentType;

            bool checkNamedEnviornmentType = bool.Parse(Configuration.Config.GetHostSetting("DataFlowStepBasedOnNamedEnvironmentType"));

            //Look at ActionType and return correct BaseAction
            BaseAction action;
            switch (actionType)
            {
                case DataActionType.S3Drop:
                    action = _datasetContext.S3DropAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.ProducerS3Drop:
                    action = _datasetContext.ProducerS3DropAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.RawStorage:
                    action = _datasetContext.RawStorageAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.QueryStorage:
                    action = _datasetContext.QueryStorageAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.ConvertParquet:
                    action = _datasetContext.ConvertToParquetAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.CopyToParquet:
                    action = _datasetContext.CopyToParquetAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.UncompressZip:
                    action = _datasetContext.UncompressZipAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.GoogleApi:
                    action = _datasetContext.GoogleApiAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.GoogleBigQueryApi:
                    action = _datasetContext.GoogleBigQueryApiAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.GoogleSearchConsoleApi:
                    action = _datasetContext.GoogleSearchConsoleApiAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.ClaimIq:
                    action = _datasetContext.ClaimIQAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.UncompressGzip:
                    action = _datasetContext.UncompressGzipAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.FixedWidth:
                    action = _datasetContext.FixedWidthAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.XML:
                    action = _datasetContext.XMLAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.JsonFlattening:
                    action = _datasetContext.JsonFlatteningAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    break;
                case DataActionType.SchemaLoad:

                    action = _datasetContext.SchemaLoadAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
                    
                    DataFlowStep schemaLoadStep = MapToDataFlowStep(df, action, actionType);
                    List<SchemaMap> schemaMapList = new List<SchemaMap>();
                    foreach (SchemaMapDto mapDto in dto.SchemaMap.Where(w => !w.IsDeleted))
                    {
                        schemaMapList.Add(MapToSchemaMap(mapDto, schemaLoadStep));
                    }
                    schemaLoadStep.SchemaMappings = schemaMapList;
                    
                    return schemaLoadStep;

                //TODO: REMOVE THIS ENTIRELY.  NOT CALLED ANYNORE SINCE SCHEMA MAP IS V2 and NO LONGER EVEN CALLED, BONUS AFTER TOPIC NAME IS WORKING
                case DataActionType.SchemaMap:
                    action = _datasetContext.SchemaMapAction.GetAction(_dataFeatures, isHumanResources, datasetNamedEnviornmentType, checkNamedEnviornmentType);
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

        private void SetTrigger(DataFlowStep step)
        {
            if (step.DataAction_Type_Id == DataActionType.S3Drop)
            {
                step.TriggerKey = $"droplocation/{Configuration.Config.GetHostSetting("S3DataPrefix")}{step.DataFlow.FlowStorageCode}/";
                SetTriggerBucketForS3DropLocation(step);
            }
            else if (step.DataAction_Type_Id == DataActionType.ProducerS3Drop)
            {
                step.TriggerKey = $"drop/{step.DataFlow.SaidKeyCode.ToUpper()}/{step.DataFlow.NamedEnvironment.ToUpper()}/{step.DataFlow.FlowStorageCode}/";
                SetTriggerBucketForS3DropLocation(step);
            }
            else
            {
                string datasetSaidAsset = GetDatasetSaidAsset(step.DataFlow.Id);
                string datasetNamedEnv = GetDatasetNamedEnvironment(step.DataFlow.Id);
                string triggerPrefix = $"{GlobalConstants.DataFlowTargetPrefixes.TEMP_FILE_PREFIX}{step.Action.TriggerPrefix}{datasetSaidAsset}/{datasetNamedEnv}/{step.DataFlow.FlowStorageCode}/";
                step.TriggerKey = triggerPrefix;
                step.TriggerBucket = step.Action.TargetStorageBucket;
            }
        }

        private void SetTriggerBucketForS3DropLocation(DataFlowStep step)
        {
            step.TriggerBucket = string.IsNullOrWhiteSpace(step.DataFlow.UserDropLocationBucket)
                ? step.Action.TargetStorageBucket
                : step.DataFlow.UserDropLocationBucket;
        }

        private void SetTarget(DataFlowStep step)
        {
            switch (step.DataAction_Type_Id)
            {
                case DataActionType.None:
                    break;
                //These send output to schema aware storage
                case DataActionType.QueryStorage:
                case DataActionType.ConvertParquet:
                case DataActionType.CopyToParquet:
                    string schemaStorageCode = GetSchemaStorageCodeForDataFlow(step.DataFlow.Id);
                    step.TargetPrefix = $"{step.Action.TargetStoragePrefix}{GetDatasetSaidAsset(step.DataFlow.Id)}/{GetDatasetNamedEnvironment(step.DataFlow.Id)}/{schemaStorageCode}/";
                    step.TargetBucket = step.Action.TargetStorageBucket;
                    break;
                //These sent output a step specific location along with down stream dependent steps
                case DataActionType.RawStorage:
                    step.TargetPrefix = $"{step.Action.TargetStoragePrefix}{GetDatasetSaidAsset(step.DataFlow.Id)}/{GetDatasetNamedEnvironment(step.DataFlow.Id)}/{step.DataFlow.FlowStorageCode}/";
                    step.TargetBucket = step.Action.TargetStorageBucket;
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
                    step.TargetBucket = null;
                    break;
                default:
                    break;
            }
        }

        private void SetSourceDependency(DataFlowStep step, DataFlowStep previousStep)
        {
            step.SourceDependencyPrefix = previousStep?.TriggerKey;
            step.SourceDependencyBucket = previousStep?.TriggerBucket;
        }

        /// <summary>
        /// Return SAID keycode for Dataset associated with dataflow
        /// </summary>
        private string GetDatasetSaidAsset(int dataflowId)
        {            
            int datasetId = _datasetContext.DataFlow
                            .Where(w => w.Id == dataflowId)
                            .Select(s => s.DatasetId)
                            .FirstOrDefault();
            string saidAsset = _datasetContext.Datasets
                            .Where(w => w.DatasetId == datasetId)
                            .Select(s => s.Asset.SaidKeyCode)
                            .FirstOrDefault();
            return saidAsset;
        }

        /// <summary>
        /// Return Named Environment for Dataset associated with dataflow
        /// </summary>
        private string GetDatasetNamedEnvironment(int dataflowId)
        {
            int datasetId = _datasetContext.DataFlow
                            .Where(w => w.Id == dataflowId)
                            .Select(s => s.DatasetId)
                            .FirstOrDefault();
            string namedEnvironment = _datasetContext.Datasets
                            .Where(w => w.DatasetId == datasetId)
                            .Select(s => s.NamedEnvironment)
                            .FirstOrDefault();
            return namedEnvironment;
        } 

        #region SchemaFlowMappings

        //TODO: REMOVE THIS ENTIRELY.  NOT CALLED ANYNORE SINCE SCHEMA MAP IS V2 and NO LONGER EVEN CALLED
        private DataFlow MapToDataFlow(FileSchema scm)
        {
            // This method maps Schema flow for given dataset schema
            //   The assumption is these dataflows are always of type DSC Push
            DataFlow df = new DataFlow
            {
                Id = 0,
                Name = GenerateDataFlowNameForFileSchema(scm),
                CreatedDTM = DateTime.Now,
                CreatedBy = _userService.GetCurrentUser().AssociateId,
                FlowStorageCode = _datasetContext.GetNextDataFlowStorageCDE(),
                DeleteIssueDTM = DateTime.MaxValue,
                ObjectStatus = GlobalEnums.ObjectStatusEnum.Active,
                IngestionType = (int)GlobalEnums.IngestionType.DFS_Drop
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

            if (df.ShouldCreateDFSDropLocations(_dataFeatures))
            {
                //Add default DFS drop location for data flow
                List<DataSource> srcList = _datasetContext.DataSources.ToList();
                _ = _jobService.CreateDfsRetrieverJob(df, srcList.First(w => w.SourceType == GlobalConstants.DataSourceDiscriminator.DEFAULT_DATAFLOW_DFS_DROP_LOCATION));
            }

            //Generate ingestion steps (get file to raw location)
            AddDataFlowStep(dto, df, DataActionType.S3Drop);
            AddDataFlowStep(dto, df, DataActionType.RawStorage);

            //Generate preprocessing for file types (i.e. fixedwidth, csv, json, etc...)
            MapPreProcessingSteps(scm, dto, df);

            //Generate DSC registering step
            AddDataFlowStep(dto, df, DataActionType.SchemaLoad);
            AddDataFlowStep(dto, df, DataActionType.QueryStorage);

            //Generate consumption layer steps
            if (scm.Extension.Name == GlobalConstants.ExtensionNames.PARQUET)
            {
                AddDataFlowStep(dto, df, DataActionType.CopyToParquet);
            }
            else
            {
                AddDataFlowStep(dto, df, DataActionType.ConvertParquet);
            }

        }

        private void MapPreProcessingSteps(FileSchema scm, DataFlowDto dto, DataFlow df)
        {
            if (scm.Extension.Name.ToUpper() == GlobalConstants.ExtensionNames.FIXEDWIDTH)
            {
                AddDataFlowStep(dto, df, DataActionType.FixedWidth);
            }
            else if (scm.Extension.Name.ToUpper() == GlobalConstants.ExtensionNames.XML)
            {
                AddDataFlowStep(dto, df, DataActionType.XML);
            }
            else if (scm.Extension.Name.ToUpper() == GlobalConstants.ExtensionNames.JSON && !String.IsNullOrWhiteSpace(scm.SchemaRootPath))
            {
                AddDataFlowStep(dto, df, DataActionType.JsonFlattening);
            }
        }

        
        #endregion

        #endregion
    }
}
