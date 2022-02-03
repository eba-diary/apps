﻿using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Interfaces.QuartermasterRestClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hangfire;

namespace Sentry.data.Core
{
    public class DataFlowService : IDataFlowService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;
        private readonly IJobService _jobService;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly ISecurityService _securityService;
        private readonly IQuartermasterService _quartermasterService;
        private readonly IDataFeatures _dataFeatures;
        private readonly IBackgroundJobClient _hangfireBackgroundJobClient;

        public DataFlowService(IDatasetContext datasetContext, 
            IUserService userService, IJobService jobService, IS3ServiceProvider s3ServiceProvider,
            ISecurityService securityService, IQuartermasterService quartermasterService, 
            IDataFeatures dataFeatures, IBackgroundJobClient backgroundJobClient)
        {
            _datasetContext = datasetContext;
            _userService = userService;
            _jobService = jobService;
            _s3ServiceProvider = s3ServiceProvider;
            _securityService = securityService;
            _quartermasterService = quartermasterService;
            _dataFeatures = dataFeatures;
            _hangfireBackgroundJobClient = backgroundJobClient;
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
        /// Will enqueue a hangfire job, for each id in producerDataFlowIds,
        ///   that will run on hangfire background server and peform
        ///   the dataflow upgrade.
        /// </summary>
        /// <param name="producerDataFlowIds"></param>
        /// <remarks> This will serves an Admin only funtionlaity within DataFlow API </remarks>
        public void UpgradeDataFlows(int[] producerDataFlowIds)
        {
            Logger.Info($"<{nameof(DataFlowService)}-{nameof(UpgradeDataFlows)}> Method Start");
            foreach (int producerDataFlowId in producerDataFlowIds)
            {
                _hangfireBackgroundJobClient.Enqueue<DataFlowService>(x => x.UpgradeDataFlow(producerDataFlowId));
            }
            Logger.Info($"<{nameof(DataFlowService)}-{nameof(UpgradeDataFlows)}> Method End");
        }

        /// <summary>
        /// Upgrades a producer dataflow to a single dataflow configuration (CLA-3332)
        /// </summary>
        /// <param name="producerDataFlowId"></param>
        /// <remarks>
        /// This method will be triggered by Hangfire.  
        /// Added the AutomaticRetry attribute to ensure retries do not occur for this method.
        /// https://docs.hangfire.io/en/latest/background-processing/dealing-with-exceptions.html
        /// </remarks>
        [AutomaticRetry(Attempts = 0)]
        public void UpgradeDataFlow(int producerDataFlowId)
        {
            Logger.Info($"<{nameof(DataFlowService)}-{nameof(UpgradeDataFlow)}> Method Start");

            DataFlow producerDataFlow = _datasetContext.GetById<DataFlow>(producerDataFlowId);

            if (producerDataFlow == null)
            {
                throw new DataFlowNotFound("Invalid dataflow id");
            }

            if (producerDataFlow.ObjectStatus != GlobalEnums.ObjectStatusEnum.Active)
            {
                throw new ArgumentException("Only active producer dataflows can be upgraded");
            }

            /************************************************************************************************  
             *  Do not upgrade Producer Dataflows if the following are true:
             *      1.) Dataflow name starts with "FileSchemaFlow"
             *      2.) Dataflow does not contain a dataflowstep of type SchemaMap
             *************************************************************************************************/
            DataFlowStep schemaMapStep = producerDataFlow.Steps.FirstOrDefault(w => w.DataAction_Type_Id == DataActionType.SchemaMap);
            if (schemaMapStep == null || producerDataFlow.Name.StartsWith("FileSchemaFlow"))
            {
                throw new ArgumentException("Only producer dataflows can be upgraded");
            }

            /************************************************************************************************  
             *  Only convert dataflows which map to single schema.  Dataflows that "Fan out" to multiple schema
             *      will need to be handled on a case by case basis
             *************************************************************************************************/
            if (schemaMapStep.SchemaMappings.Count > 1)
            {
                throw new ArgumentException("Only producer dataflows with single schema map can be upgraded");
            }

            DataFlowDto dto = GetDataFlowDto(producerDataFlowId);

            /************************************************************************************************
             *  With CLA3332_ConsolidateDataflows = True, the UpdateandSaveDataflow()
             *    Assumes the DataFlowDto has DatasetId set.  Since this method is converting from old
             *    to new world, we need to set DataFlowDto.DatasetId to appropriate value
             ************************************************************************************************/
            dto.DatasetId = schemaMapStep.SchemaMappings.First().Dataset.DatasetId;

            Logger.Info($"<{nameof(DataFlowService)}-{nameof(UpgradeDataFlow)}> Starting dataflow upgrade");

            /**********************************************************************************
             *  During conversion, we will create new dataflow and not delete existing dataflow.
             *  This allows data producers not be tightly coupled with the conversion effort.
             **********************************************************************************/
            int newId = UpdateandSaveDataFlow(dto, false);

            Logger.Info($"<{nameof(DataFlowService)}-{nameof(UpgradeDataFlow)}> Completed dataflow upgrade (original:{producerDataFlowId}:::new:{newId}");





            Logger.Info($"<{nameof(DataFlowService)}-{nameof(UpgradeDataFlow)}> Method End");
        }

        /// <summary>
        /// Will enqueue a hangfire job, for each id in idList,
        ///   that will run on hangfire background server and peform
        ///   the dataflow delete.
        /// </summary>
        /// <param name="idList"></param>
        /// <remarks> This will serves an Admin only funtionlaity within DataFlow API </remarks>
        public void DeleteDataFlows(int[] idList)
        {
            Logger.Info($"<{nameof(DataFlowService)}-{nameof(DeleteDataFlows)}> Method Start");

            IApplicationUser user = _userService.GetCurrentUser();

            foreach (int dataflow in idList)
            {
                _hangfireBackgroundJobClient.Enqueue<DataFlowService>(x => x.Delete(dataflow, user.AssociateId, true));
            }
            Logger.Info($"<{nameof(DataFlowService)}-{nameof(DeleteDataFlows)}> Method End");
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
            if (_dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue() &&
                _datasetContext.DataFlow.Any(df => df.DatasetId == dto.SchemaMap.First().DatasetId &&
                                                   df.SchemaId == dto.SchemaMap.First().SchemaId &&
                                                   df.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active))
            {
                throw new SchemaInUseException($"Schema ID {dto.SchemaMap.First().SchemaId} is already associated to another DataFlow.");
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
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dfDto"></param>
        /// <param name="deleteOriginal"></param>
        /// <returns></returns>
        /// <remarks>
        /// After CLA3332_ConsolidatedDataflows feature is rolled out and conversion
        /// work is completed to single dataflow structure, the deleteOriginal option
        /// should be removed.
        /// </remarks>
        public int UpdateandSaveDataFlow(DataFlowDto dfDto, bool deleteOriginal = true)
        {
            Logger.Info($"<{nameof(DataFlowService)}-{nameof(UpdateandSaveDataFlow)}> Method Start");
            /*
             *  Create new Dataflow
             *  - The incoming dto will have flowstoragecode and will
             *     be used by new dataflow as well  
            */

            DataFlow newDataFlow = CreateDataFlow(dfDto);

            /*
             *  Logically delete the existing dataflow
             *    This will take care of deleting any existing 
             *    retriever jobs and removing them from hangfire
             *  WallEService will eventually set the objects
             *    to a deleted status after a set period of time    
             */
            if (deleteOriginal)
            {
                //Delete existing dataflow
                MarkDataFlowForDeletionById(dfDto.Id);
            }

            _datasetContext.SaveChanges();

            Logger.Info($"<{nameof(DataFlowService)}-{nameof(UpdateandSaveDataFlow)}> Method Start");

            return newDataFlow.Id;
        }
        public void CreateDataFlowForSchema(FileSchema scm)
        {
            DataFlow df = MapToDataFlow(scm);

            MapDataFlowStepsForFileSchema(scm, df);

            if (!_dataFeatures.CLA3241_DisableDfsDropLocation.GetValue())
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

        /// <summary>
        /// Delete all data flows for a given <see cref="FileSchema"/> object.
        /// </summary>
        /// <param name="scm">The FileSchema object</param>
        /// <param name="logicalDelete">True to logically delete the dataflow; false to physically delete it</param>
        public void DeleteFlowsByFileSchema(FileSchema scm, bool logicalDelete = true)
        {
            Logger.Info($"{nameof(DataFlowService).ToLower()}_{nameof(DeleteFlowsByFileSchema).ToLower()} Method Start");

            DeleteSchemaFlowByFileSchema(scm, logicalDelete);

            /* Get associated producer flow(s) */
            List<int> producerDataflowIdList = GetProducerFlowsToBeDeletedBySchemaId(scm.SchemaId);

            Logger.Info($"{producerDataflowIdList.Count} dataflows identified");

            if (logicalDelete)
            {
                /* Mark associated producer dataflows for deletion */
                MarkDataFlowForDeletionById(producerDataflowIdList);
            }
            else
            {
                foreach (int flowId in producerDataflowIdList)
                {
                    IApplicationUser user = _userService.GetCurrentUser();
                    Delete(flowId, user.AssociateId, false);
                }
            }

            Logger.Info($"{nameof(DataFlowService).ToLower()}_{nameof(DeleteFlowsByFileSchema).ToLower()} Method End");
        }

        /// <summary>
        /// Finds a Schema Flow associated with the provided <see cref="FileSchema"/>, and deletes it.
        /// The method is OK if no Schema Flow is associated/found.
        /// </summary>
        /// <param name="scm">The FileSchema object</param>
        /// <param name="logicalDelete">True to logically delete the dataflow; false to physically delete it</param>
        /// <remarks>
        /// Schema Flows will stop being created as seperate flows as part of CLA-3332. However, this
        /// code is still needed for existing data flows - until they're converted.
        /// </remarks>
        private void DeleteSchemaFlowByFileSchema(FileSchema scm, bool logicalDelete)
        {
            Logger.Info($"{nameof(DataFlowService).ToLower()}_{nameof(DeleteSchemaFlowByFileSchema).ToLower()} Method Start");
            /* Get Schema Flow */
            var schemaflowName = GetDataFlowNameForFileSchema(scm);
            DataFlow schemaFlow = _datasetContext.DataFlow.FirstOrDefault(w => w.Name == schemaflowName);

            //some legacy dataset\schema may not have associated schema flow
            if (schemaFlow == null)
            {
                Logger.Debug($"Schema Flow not found by name, attempting to detect by id...");

                SchemaMap mappedStep = _datasetContext.SchemaMap.SingleOrDefault(w => w.MappedSchema.SchemaId == scm.SchemaId && w.DataFlowStepId.Action.Name == "Schema Load" && w.DataFlowStepId.DataFlow.Name.StartsWith("FileSchema"));
                if (mappedStep != null)
                {
                    Logger.Debug($"detected schema flow by Id");
                    schemaFlow = mappedStep.DataFlowStepId.DataFlow;
                }
                else
                {
                    Logger.Debug($"schema flow not detected by id");
                    Logger.Debug($"no schema flow associated with schema");
                }
            }

            if (schemaFlow != null)
            {
                Logger.Debug($"schema flow name: {schemaFlow.Name}");

                if (logicalDelete)
                {
                    /* Mark schema dataflow for deletion */
                    MarkDataFlowForDeletionById(schemaFlow.Id);
                }
                else
                {
                    IApplicationUser user = _userService.GetCurrentUser();
                    Delete(schemaFlow.Id, user.AssociateId, false);
                }
            }

            Logger.Info($"{nameof(DataFlowService).ToLower()}_{nameof(DeleteSchemaFlowByFileSchema).ToLower()} Method End");
        }

        /// <summary>
        /// Marks dataflows, along with associated retriever jobs, as Pending Delete
        /// </summary>
        /// <param name="idList"></param>
        private void MarkDataFlowForDeletionById(List<int> idList)
        {
            foreach (int id in idList)
            {
                MarkDataFlowForDeletionById(id);
            }
        }

        /// <summary>
        /// Marks dataflow, along with associated retriever jobs, as Pending Delete
        /// </summary>
        /// <param name="id">DataFlow Id</param>
        private void MarkDataFlowForDeletionById(int id)
        {
            Logger.Info($"<{nameof(DataFlowService)}-{nameof(MarkDataFlowForDeletionById)}> Method Start");

            //Get DataFlow and mark for deletion
            DataFlow dataFlow = _datasetContext.GetById<DataFlow>(id);
            dataFlow.ObjectStatus = GlobalEnums.ObjectStatusEnum.Pending_Delete;
            dataFlow.DeleteIssuer = _userService.GetCurrentUser().AssociateId;
            dataFlow.DeleteIssueDTM = DateTime.Now;

            //Get associated retrieverjobs and mark for deletion
            List<int> jobList = _datasetContext.RetrieverJob.Where(w => w.DataFlow.Id == id).Select(s => s.Id).ToList();
            if (jobList.Any())
            {
                _jobService.DeleteJob(jobList);
            }

            Logger.Info($"<{nameof(DataFlowService)}-{nameof(MarkDataFlowForDeletionById)}> Method End");
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

        /// <summary>
        /// This will set ObjectStatus = Deleted for specified dataflow.  In addition,
        ///   will find any retrieverjobs, associated with specified dataflow, and 
        ///   set its ObjectStatus = Deleted.
        /// </summary>
        /// <param name="dataFlowId"></param>
        /// <param name="commitChanges">True: method will save changes to DB, False: relies on calling method to save changes</param>
        /// <remarks>
        /// This method can be triggered by Hangfire.  
        /// Added the AutomaticRetry attribute to ensure retries do not occur for this method.
        /// https://docs.hangfire.io/en/latest/background-processing/dealing-with-exceptions.html
        /// </remarks>
        [AutomaticRetry(Attempts = 0)]
        public void Delete(int dataFlowId, string userId, bool commitChanges = false)
        {
            Logger.Debug($"{nameof(DataFlowService).ToLower()}_{nameof(Delete).ToLower()} Method Start");

            Logger.Info($"{nameof(DataFlowService).ToLower()}_{nameof(Delete).ToLower()} - dataflowid:{dataFlowId}");

            //Find DataFlow
            DataFlow flow = _datasetContext.GetById<DataFlow>(dataFlowId);


            if (flow == null)
            {
                Logger.Debug($"{nameof(DataFlowService).ToLower()}_{nameof(Delete).ToLower()} DataFlow not found - dataflowid:{dataFlowId}");
                throw new DataFlowNotFound();
            }
            //If dataflow is already deleted then exit
            else if (flow.ObjectStatus == GlobalEnums.ObjectStatusEnum.Deleted)
            {
                Logger.Debug($"{nameof(DataFlowService).ToLower()}_{nameof(Delete).ToLower()} DataFlow already deleted - dataflowid:{dataFlowId}");
            }
            else
            {
                //Mark dataflow deleted
                flow.ObjectStatus = GlobalEnums.ObjectStatusEnum.Deleted;
                if (string.IsNullOrEmpty(flow.DeleteIssuer))
                {
                    flow.DeleteIssuer = userId;
                }

                //Only comparing date since the milliseconds percision are different, therefore, never evaluates true
                //  https://stackoverflow.com/a/44324883
                if (DateTime.MaxValue.Date == flow.DeleteIssueDTM.Date)
                {
                    flow.DeleteIssueDTM = DateTime.Now;
                }

                //Delete associated retriever jobs
                _jobService.DeleteJobByDataFlowId(dataFlowId, deleteIssuerId:userId, logicalDelete:false);

                if (commitChanges)
                {
                    _datasetContext.SaveChanges();
                }
            }            

            Logger.Debug($"{nameof(DataFlowService).ToLower()}_{nameof(Delete).ToLower()} Method End");
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

        public async Task<ValidationException> Validate(DataFlowDto dfDto)
        {
            ValidationResults results = new ValidationResults();
            if (dfDto.Id == 0 &&_datasetContext.DataFlow.Any(w => w.Name == dfDto.Name))
            {
                results.Add(DataFlow.ValidationErrors.nameMustBeUnique, "Dataflow name is already used");
            }

            //Validate the Named Environment selection using the QuartermasterService
            results.MergeInResults(await _quartermasterService.VerifyNamedEnvironmentAsync(dfDto.SaidKeyCode, dfDto.NamedEnvironment, dfDto.NamedEnvironmentType).ConfigureAwait(false));

            return new ValidationException(results);
        }

        #region Private Methods
        
        private DataFlow CreateDataFlow(DataFlowDto dto)
        {
            Logger.Info($"<{nameof(DataFlowService)}-{nameof(CreateDataFlow)}> Method Start");

            DataFlow df = MapToDataFlow(dto);

            switch (dto.IngestionType)
            {
                case (int)GlobalEnums.IngestionType.User_Push:
                    MapDataFlowStepsForPush(dto, df);
                    break;
                case (int)GlobalEnums.IngestionType.DSC_Pull:
                    MapDataFlowStepsForPull(dto, df);
                    break;
                default:
                    break;
            }

            Logger.Info($"<{nameof(DataFlowService)}-{nameof(CreateDataFlow)}> Method End");
            return df;
        }

        private DataFlow MapToDataFlow(DataFlowDto dto)
        {
            Logger.Info($"<{nameof(DataFlowService)}-{nameof(MapToDataFlow)}> Method Start");
            DataFlow df = new DataFlow
            {
                Name = dto.Name,
                CreatedDTM = DateTime.Now,
                CreatedBy = dto.CreatedBy,
                Questionnaire = dto.DFQuestionnaire,
                FlowStorageCode = (!_dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue()) 
                                    ? _datasetContext.GetNextDataFlowStorageCDE() 
                                    : _datasetContext.FileSchema.Where(x => x.SchemaId == dto.SchemaMap.First().SchemaId).Select(s => s.StorageCode).FirstOrDefault(),
                SaidKeyCode = dto.SaidKeyCode,
                ObjectStatus = Core.GlobalEnums.ObjectStatusEnum.Active,
                DeleteIssuer = dto.DeleteIssuer,
                DeleteIssueDTM = DateTime.MaxValue,
                IngestionType = dto.IngestionType,
                IsDecompressionRequired = dto.IsCompressed,
                CompressionType = dto.CompressionType,
                IsPreProcessingRequired = dto.IsPreProcessingRequired,
                PreProcessingOption = (int)dto.PreProcessingOption,
                NamedEnvironment = dto.NamedEnvironment,
                NamedEnvironmentType = dto.NamedEnvironmentType
            };

            if (_dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue())
            {
                df.DatasetId = dto.SchemaMap.First().DatasetId;
                df.SchemaId = dto.SchemaMap.First().SchemaId;
            }

            _datasetContext.Add(df);

            Logger.Info($"<{nameof(DataFlowService)}-{nameof(MapToDataFlow)}> Method End");
            return df;
        }

        private void MapDataFlowStepsForPush(DataFlowDto dto, DataFlow df)
        {
            Logger.Info($"<{nameof(DataFlowService)}-{nameof(MapDataFlowStepsForPush)}> Method Start");
            //This type of dataflow does not need to worry about retrieving data from external sources
            // Data will be pushed by user to S3 and\or DFS drop locations

            if (!_dataFeatures.CLA3241_DisableDfsDropLocation.GetValue())
            {
                //Add default DFS drop location for data flow
                List<DataSource> srcList = _datasetContext.DataSources.ToList();
                RetrieverJob dfsDataFlowBasic = _jobService.InstantiateJobsForCreation(df, srcList.First(w => w.SourceType == GlobalConstants.DataSoureDiscriminator.DEFAULT_DATAFLOW_DFS_DROP_LOCATION));
                _datasetContext.Add(dfsDataFlowBasic);
                _jobService.CreateDropLocation(dfsDataFlowBasic);
            }

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
                    default:
                        break;
                }
            }

            //Feature flag = false, add schema map step
            //Feature flag = true, do no add schema map step
            if (!_dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue())
            {
                //Generate Schema Map step to send files to schema specific data flow
                AddDataFlowStep(dto, df, DataActionType.SchemaMap);
            }
            else
            {

                FileSchema scm = _datasetContext.GetById<FileSchema>(dto.SchemaMap.First().SchemaId);

                //Generate preprocessing for file types (i.e. fixedwidth, csv, json, etc...)
                MapPreProcessingSteps(scm, dto, df);

                //Generate DSC registering step
                AddDataFlowStep(dto, df, DataActionType.SchemaLoad);
                AddDataFlowStep(dto, df, DataActionType.QueryStorage);

                ////Generate consumption layer steps
                AddDataFlowStep(dto, df, DataActionType.ConvertParquet);
            }

            Logger.Info($"<{nameof(DataFlowService)}-{nameof(MapDataFlowStepsForPush)}> Method End");
        }

        private void MapDataFlowStepsForPull(DataFlowDto dto, DataFlow df)
        {
            Logger.Info($"<{nameof(DataFlowService)}-{nameof(MapDataFlowStepsForPull)}> Method Start");

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
                switch (dto.PreProcessingOption)
                {
                    case (int)DataFlowPreProcessingTypes.googleapi:
                        AddDataFlowStep(dto, df, DataActionType.GoogleApi);
                        break;
                    case (int)DataFlowPreProcessingTypes.claimiq:
                        AddDataFlowStep(dto, df, DataActionType.ClaimIq);
                        break;
                    default:
                        break;
                }
            }


            //Feature flag = false, add schema map step
            //Feature flag = true, do no add schema map step
            if (!_dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue())
            {
                //Generate Schema Map step to send files to schema specific data flow
                AddDataFlowStep(dto, df, DataActionType.SchemaMap);
            }
            else
            {

                FileSchema scm = _datasetContext.GetById<FileSchema>(dto.SchemaMap.First().SchemaId);

                //Generate preprocessing for file types (i.e. fixedwidth, csv, json, etc...)
                MapPreProcessingSteps(scm, dto, df);

                //Generate DSC registering step
                AddDataFlowStep(dto, df, DataActionType.SchemaLoad);
                AddDataFlowStep(dto, df, DataActionType.QueryStorage);

                ////Generate consumption layer steps
                AddDataFlowStep(dto, df, DataActionType.ConvertParquet);
            }

            Logger.Info($"<{nameof(DataFlowService)}-{nameof(MapDataFlowStepsForPull)}> Method End");

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
            dto.MappedSchema = (_dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue() && df.SchemaId != 0) ? new List<int>() { df.SchemaId } : GetMappedFileSchema(df.Id);
            dto.AssociatedJobs = GetExternalRetrieverJobs(df.Id);
            dto.ObjectStatus = df.ObjectStatus;
            dto.DeleteIssuer = df.DeleteIssuer;
            dto.DeleteIssueDTM = df.DeleteIssueDTM;
            dto.IngestionType = df.IngestionType;
            dto.IsCompressed = df.IsDecompressionRequired;
            dto.CompressionType = df.CompressionType;
            dto.IsPreProcessingRequired = df.IsPreProcessingRequired;
            dto.PreProcessingOption = df.PreProcessingOption;
            dto.SaidKeyCode = df.SaidKeyCode;
            dto.NamedEnvironment = df.NamedEnvironment;
            dto.NamedEnvironmentType = df.NamedEnvironmentType;

            List<SchemaMapDto> scmMapDtoList = new List<SchemaMapDto>();
            foreach (DataFlowStep step in df.Steps.Where(w => w.SchemaMappings != null && w.SchemaMappings.Any()))
            {
                foreach (SchemaMap map in step.SchemaMappings)
                {
                    scmMapDtoList.Add(map.ToDto());
                }
            }
            dto.SchemaMap = scmMapDtoList;

            dto.DatasetId = (_dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue() && df.DatasetId != 0) ? df.DatasetId : dto.SchemaMap.FirstOrDefault().DatasetId;
            dto.SchemaId = (_dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue() && df.SchemaId != 0) ? df.SchemaId : dto.SchemaMap.FirstOrDefault().SchemaId;
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

            // If WORKDAY (HR category) work needs to go before CLA3332_ConsolidatedDataFlows,
            //   then we need to determine datasetid associated with dto.SchemaMap.First().SchemaId
            if (_dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue())
            {
                //Take DatasetId and figure out if Category = HR
                Dataset ds = _datasetContext.GetById<Dataset>(dto.DatasetId);
                if (ds.DatasetCategories.Any(w => w.AbbreviatedName == "HR"))
                {
                    isHumanResources = true;
                }
            }

            //Look at ActionType and return correct BaseAction
            BaseAction action;
            switch (actionType)
            {
                case DataActionType.S3Drop:
                    action = _datasetContext.S3DropAction.GetAction(_dataFeatures, isHumanResources);
                    break;
                case DataActionType.ProducerS3Drop:
                    action = _datasetContext.ProducerS3DropAction.GetAction(_dataFeatures, isHumanResources);
                    break;
                case DataActionType.RawStorage:
                    action = _datasetContext.RawStorageAction.GetAction(_dataFeatures, isHumanResources);
                    break;
                case DataActionType.QueryStorage:
                    action = _datasetContext.QueryStorageAction.GetAction(_dataFeatures, isHumanResources);
                    break;
                case DataActionType.ConvertParquet:
                    action = _datasetContext.ConvertToParquetAction.GetAction(_dataFeatures, isHumanResources);
                    break;
                case DataActionType.UncompressZip:
                    action = _datasetContext.UncompressZipAction.GetAction(_dataFeatures, isHumanResources);
                    break;
                case DataActionType.GoogleApi:
                    action = _datasetContext.GoogleApiAction.GetAction(_dataFeatures, isHumanResources);
                    break;
                case DataActionType.ClaimIq:
                    action = _datasetContext.ClaimIQAction.GetAction(_dataFeatures, isHumanResources);
                    break;
                case DataActionType.UncompressGzip:
                    action = _datasetContext.UncompressGzipAction.GetAction(_dataFeatures, isHumanResources);
                    break;
                case DataActionType.FixedWidth:
                    action = _datasetContext.FixedWidthAction.GetAction(_dataFeatures, isHumanResources);
                    break;
                case DataActionType.XML:
                    action = _datasetContext.XMLAction.GetAction(_dataFeatures, isHumanResources);
                    break;
                case DataActionType.JsonFlattening:
                    action = _datasetContext.JsonFlatteningAction.GetAction(_dataFeatures, isHumanResources);
                    break;
                case DataActionType.SchemaLoad:

                    action = _datasetContext.SchemaLoadAction.GetAction(_dataFeatures, isHumanResources);
                    
                    DataFlowStep schemaLoadStep = MapToDataFlowStep(df, action, actionType);
                    List<SchemaMap> schemaMapList = new List<SchemaMap>();
                    foreach (SchemaMapDto mapDto in dto.SchemaMap.Where(w => !w.IsDeleted))
                    {
                        schemaMapList.Add(MapToSchemaMap(mapDto, schemaLoadStep));
                    }
                    schemaLoadStep.SchemaMappings = schemaMapList;
                    
                    return schemaLoadStep;

                case DataActionType.SchemaMap:
                    action = _datasetContext.SchemaMapAction.GetAction(_dataFeatures, isHumanResources);
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
                step.TriggerKey = _dataFeatures.CLA3240_UseDropLocationV2.GetValue()
                    ? $"drop/{step.DataFlow.SaidKeyCode.ToUpper()}/{step.DataFlow.NamedEnvironment.ToUpper()}/{step.DataFlow.FlowStorageCode}/"
                    : $"droplocation/data/{step.DataFlow.SaidKeyCode}/{step.DataFlow.FlowStorageCode}/";
                SetTriggerBucketForS3DropLocation(step);
            }
            else
            {
                string triggerPrefix = _dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue()
                    ? $"{GlobalConstants.DataFlowTargetPrefixes.TEMP_FILE_PREFIX}{step.Action.TargetStoragePrefix}{GetDatasetSaidAsset(step.DataFlow.Id)}/{GetDatasetNamedEnvironment(step.DataFlow.Id)}/{step.DataFlow.FlowStorageCode}/"
                    : $"{GlobalConstants.DataFlowTargetPrefixes.TEMP_FILE_PREFIX}{step.Action.TargetStoragePrefix}{Configuration.Config.GetHostSetting("S3DataPrefix")}{step.DataFlow.FlowStorageCode}/";
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
                    string schemaStorageCode = GetSchemaStorageCodeForDataFlow(step.DataFlow.Id);
                    step.TargetPrefix = _dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue()
                        ? $"{step.Action.TargetStoragePrefix}{GetDatasetSaidAsset(step.DataFlow.Id)}/{GetDatasetNamedEnvironment(step.DataFlow.Id)}/{schemaStorageCode}/"
                        : step.Action.TargetStoragePrefix + $"{Configuration.Config.GetHostSetting("S3DataPrefix")}{schemaStorageCode}/";
                    step.TargetBucket = step.Action.TargetStorageBucket;
                    break;
                //These sent output a step specific location along with down stream dependent steps
                case DataActionType.RawStorage:
                    step.TargetPrefix = _dataFeatures.CLA3332_ConsolidatedDataFlows.GetValue()
                        ? $"{step.Action.TargetStoragePrefix}{GetDatasetSaidAsset(step.DataFlow.Id)}/{GetDatasetNamedEnvironment(step.DataFlow.Id)}/{step.DataFlow.FlowStorageCode}/"
                        : step.Action.TargetStoragePrefix + $"{Configuration.Config.GetHostSetting("S3DataPrefix")}{step.DataFlow.FlowStorageCode}/";
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
                            .Select(s => s.SAIDAssetKeyCode)
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
                IngestionType = (int)GlobalEnums.IngestionType.User_Push
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

            if (!_dataFeatures.CLA3241_DisableDfsDropLocation.GetValue())
            {
                //Add default DFS drop location for data flow
                List<DataSource> srcList = _datasetContext.DataSources.ToList();
                RetrieverJob dfsDataFlowBasic = _jobService.InstantiateJobsForCreation(df, srcList.First(w => w.SourceType == GlobalConstants.DataSoureDiscriminator.DEFAULT_DATAFLOW_DFS_DROP_LOCATION));
                _datasetContext.Add(dfsDataFlowBasic);
            }

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
