using Sentry.Common.Logging;
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

namespace Sentry.data.Core
{
    public class DataFlowService : IDataFlowService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;
        private readonly IJobService _jobService;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly ISecurityService _securityService;
        private readonly IClient _quartermasterClient;
        private readonly IDataFeatures _dataFeatures;

        public DataFlowService(IDatasetContext datasetContext, 
            IUserService userService, IJobService jobService, IS3ServiceProvider s3ServiceProvider,
            ISecurityService securityService, IClient quartermasterClient, IDataFeatures dataFeatures)
        {
            _datasetContext = datasetContext;
            _userService = userService;
            _jobService = jobService;
            _s3ServiceProvider = s3ServiceProvider;
            _securityService = securityService;
            _quartermasterClient = quartermasterClient;
            _dataFeatures = dataFeatures;
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
                return 0;
            }
        }

        public int UpdateandSaveDataFlow(DataFlowDto dfDto)
        {
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

            //Delete existing dataflow
            MarkDataFlowForDeletionById(dfDto.Id);
            //Delete(dfDto.Id);

            _datasetContext.SaveChanges();

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
            string methodName = MethodBase.GetCurrentMethod().Name.ToLower();
            Logger.Debug($"Start method <{methodName}>");

            DeleteSchemaFlowByFileSchema(scm, logicalDelete);

            /* Get associated producer flow(s) */
            List<int> producerDataflowIdList = GetProducerFlowsToBeDeletedBySchemaId(scm.SchemaId);

            if (logicalDelete)
            {
                /* Mark associated producer dataflows for deletion */
                MarkDataFlowForDeletionById(producerDataflowIdList);
            }
            else
            {
                foreach (int flowId in producerDataflowIdList)
                {
                    Delete(flowId);
                }
            }

            Logger.Debug($"End method <{methodName}>");
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
                    Delete(schemaFlow.Id);
                }
            }
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
            string methodName = MethodBase.GetCurrentMethod().Name.ToLower();
            Logger.Debug($"Start method <{methodName}>");

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

            Logger.Debug($"End method <{methodName}>");
        }

        private List<int> GetProducerFlowsToBeDeletedBySchemaId(int schemaId)
        {
            List<int> producerFlowIdList = new List<int>();

            /* Get producer flow Ids which map to schema */
            List<Tuple<int, int>> producerSchemaMapIds = _datasetContext.SchemaMap.Where(w => w.MappedSchema.SchemaId == schemaId && w.DataFlowStepId.DataAction_Type_Id == DataActionType.SchemaMap).Select(s => new Tuple<int, int>(s.DataFlowStepId.Id, s.DataFlowStepId.DataFlow.Id)).ToList();

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
            string methodName = MethodBase.GetCurrentMethod().Name.ToLower();
            Logger.Debug($"Start method <{methodName}>");

            Logger.Info($"dataflowservice-delete-start - dataflowid:{dataFlowId}");

            //Find DataFlow
            DataFlow flow = _datasetContext.GetById<DataFlow>(dataFlowId);

            if (flow == null)
            {
                Logger.Debug($"dataflowservice-delete DataFlow not found - dataflowid:{dataFlowId}");
                throw new DataFlowNotFound();
            }
            else
            {
                //Mark dataflow deleted
                flow.ObjectStatus = GlobalEnums.ObjectStatusEnum.Deleted;

                //Find associated retriever job
                List<int> jobList = _datasetContext.RetrieverJob.Where(w => w.DataFlow == flow).Select(s => s.Id).ToList();

                //Mark retriever jobs deleted
                Logger.Info($"dataflowservice-delete-deleteretrieverjobs - dataflowid:{flow.Id}");
                foreach (int job in jobList)
                {
                    _jobService.DeleteJob(job, false);
                }
            }

            Logger.Debug($"End method <{methodName}>");
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

            //if the asset is managed in Quartermaster, then the named environment and named environment type must be valid according to Quartermaster
            var namedEnvironmentList = (await _quartermasterClient.NamedEnvironmentsGet2Async(dfDto.SaidKeyCode, ShowDeleted11.False).ConfigureAwait(false)).ToList();
            if (namedEnvironmentList.Any())
            {
                if (!namedEnvironmentList.Any(e => e.Name == dfDto.NamedEnvironment))
                {
                    results.Add(DataFlow.ValidationErrors.namedEnvironmentInvalid, $"Named Environment provided (\"{dfDto.NamedEnvironment}\") doesn't match a Quartermaster Named Environment");
                }
                else if (namedEnvironmentList.First(e => e.Name == dfDto.NamedEnvironment).Environmenttype != dfDto.NamedEnvironmentType.ToString())
                {
                    var quarterMasterNamedEnvironmentType = namedEnvironmentList.First(e => e.Name == dfDto.NamedEnvironment).Environmenttype;
                    results.Add(DataFlow.ValidationErrors.namedEnvironmentTypeInvalid, $"Named Environment Type provided (\"{dfDto.NamedEnvironmentType}\") doesn't match Quartermaster (\"{quarterMasterNamedEnvironmentType}\")");
                }
            }

            return new ValidationException(results);
        }

        /// <summary>
        /// Given a SAID asset key code, get all the named environments from Quartermaster
        /// </summary>
        /// <param name="saidAssetKeyCode">The four-character key code for an asset</param>
        /// <returns>A list of NamedEnvironmentDto objects</returns>
        public Task<List<NamedEnvironmentDto>> GetNamedEnvironmentsAsync(string saidAssetKeyCode)
        {
            //validate parameters
            if (string.IsNullOrWhiteSpace(saidAssetKeyCode))
            {
                throw new ArgumentNullException(nameof(saidAssetKeyCode), "SAID Asset Key Code was missing.");
            }

            //the guts of the method have to be wrapped in a local function for proper async handling
            //see https://confluence.sentry.com/questions/224368523
            async Task<List<NamedEnvironmentDto>> GetNamedEnvironmentsInternalAsync()
            {
                //call Quartermaster to get list of named environments for this asset
                var namedEnvironmentList = (await _quartermasterClient.NamedEnvironmentsGet2Async(saidAssetKeyCode, ShowDeleted11.False).ConfigureAwait(false)).ToList();
                namedEnvironmentList = namedEnvironmentList.OrderBy(n => n.Name).ToList();

                //grab a config setting to see if we need to filter the named environments by a certain named environment type
                //if the config setting for "QuartermasterNamedEnvironmentTypeFilter" is blank, no filter will be applied
                var environmentTypeFilter = Configuration.Config.GetHostSetting("QuartermasterNamedEnvironmentTypeFilter");
                Func<NamedEnvironment, bool> filter = env => true;
                if (!string.IsNullOrWhiteSpace(environmentTypeFilter))
                {
                    filter = env => env.Environmenttype == environmentTypeFilter;
                }

                //map the output from Quartermaster to our Dto
                return namedEnvironmentList.Where(filter).Select(env => new NamedEnvironmentDto
                {
                    NamedEnvironment = env.Name,
                    NamedEnvironmentType = (GlobalEnums.NamedEnvironmentType)Enum.Parse(typeof(GlobalEnums.NamedEnvironmentType), env.Environmenttype)
                }).ToList();
            }
            return GetNamedEnvironmentsInternalAsync();
        }

        #region Private Methods
        
        private DataFlow CreateDataFlow(DataFlowDto dto)
        {
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

            return df;
        }

        private DataFlow MapToDataFlow(DataFlowDto dto)
        {
            DataFlow df = new DataFlow
            {
                Name = dto.Name,
                CreatedDTM = DateTime.Now,
                CreatedBy = _userService.GetCurrentUser().AssociateId,
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

            return df;
        }

        private void MapDataFlowStepsForPush(DataFlowDto dto, DataFlow df)
        {
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
            dto.DatasetId = df.DatasetId;
            dto.SchemaId = df.SchemaId;
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

            List<SchemaMapDto> scmMapDtoList = new List<SchemaMapDto>();
            foreach (DataFlowStep step in df.Steps.Where(w => w.SchemaMappings != null && w.SchemaMappings.Any()))
            {
                foreach (SchemaMap map in step.SchemaMappings)
                {
                    scmMapDtoList.Add(map.ToDto());
                }
            }
            dto.SchemaMap = scmMapDtoList;
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
            BaseAction action;
            switch (actionType)
            {
                case DataActionType.S3Drop:
                    action = _datasetContext.S3DropAction.FirstOrDefault();
                    break;
                case DataActionType.ProducerS3Drop:
                    action = _dataFeatures.CLA3240_UseDropLocationV2.GetValue()
                        ? _datasetContext.ProducerS3DropAction.GetDlstDropLocation()
                        : _datasetContext.ProducerS3DropAction.GetDataDropLocation();
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
                    ? $"{step.DataFlow.SaidKeyCode}/{step.DataFlow.FlowStorageCode}/"
                    : $"droplocation/data/{step.DataFlow.SaidKeyCode}/{step.DataFlow.FlowStorageCode}/";
                SetTriggerBucketForS3DropLocation(step);
            }
            else
            {
                step.TriggerKey = $"{GlobalConstants.DataFlowTargetPrefixes.TEMP_FILE_PREFIX}{step.Action.TargetStoragePrefix}{Configuration.Config.GetHostSetting("S3DataPrefix")}{step.DataFlow.FlowStorageCode}/";
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
                    step.TargetPrefix = step.Action.TargetStoragePrefix + $"{Configuration.Config.GetHostSetting("S3DataPrefix")}{schemaStorageCode}/";
                    step.TargetBucket = step.Action.TargetStorageBucket;
                    break;
                //These sent output a step specific location along with down stream dependent steps
                case DataActionType.RawStorage:
                    step.TargetPrefix = step.Action.TargetStoragePrefix + $"{Configuration.Config.GetHostSetting("S3DataPrefix")}{step.DataFlow.FlowStorageCode}/";
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
