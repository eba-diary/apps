using Hangfire;
using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    /// <summary>
    /// The DataApplicationService is used to control transactional boundries for
    /// any domain context changes.  Underlying IEntityServices ensure relationships
    /// between entity objects are up held.
    /// </summary>
    public class DataApplicationService : IDataApplicationService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly Lazy<IDatasetService> _datasetService;
        private readonly Lazy<IConfigService> _configService;
        private readonly Lazy<IDataFlowService> _dataFlowService;
        private readonly Lazy<IBackgroundJobClient> _hangfireBackgroundJobClient;
        private readonly Lazy<IUserService> _userService;
        private readonly Lazy<IDataFeatures> _dataFeatures;
        private readonly Lazy<ISecurityService> _securityService;
        private readonly Lazy<ISchemaService> _schemaService;
        private readonly Lazy<IJobService> _jobService;
        private readonly Lazy<IQuartermasterService> _quartermasterService;

        public DataApplicationService(IDatasetContext datasetContext,
            Lazy<IDatasetService> datasetService, Lazy<IConfigService> configService,
            Lazy<IDataFlowService> dataFlowService, Lazy<IBackgroundJobClient> hangfireBackgroundJobClient,
            Lazy<IUserService> userService, Lazy<IDataFeatures> dataFeatures,
            Lazy<ISecurityService> securityService, Lazy<ISchemaService> schemaService,
            Lazy<IJobService> jobService, Lazy<IQuartermasterService> quartermasterService)
        {
            _datasetContext = datasetContext;
            _datasetService = datasetService;
            _configService = configService;
            _dataFlowService = dataFlowService;
            _hangfireBackgroundJobClient = hangfireBackgroundJobClient;
            _userService = userService;
            _dataFeatures = dataFeatures;
            _securityService = securityService;
            _schemaService = schemaService;
            _jobService = jobService;
            _quartermasterService = quartermasterService;
        }

        #region Private Properties
        private IDatasetService DatasetService
        {
            get { return _datasetService.Value; }
        }
        private IConfigService ConfigService
        {
            get { return _configService.Value; }
        }
        private IDataFlowService DataFlowService
        {
            get { return _dataFlowService.Value; }
        }
        private IBackgroundJobClient BackgroundJobClient
        {
            get { return _hangfireBackgroundJobClient.Value; }
        }
        private IUserService UserService
        {
            get { return _userService.Value; }
        }
        private IDataFeatures DataFeatures
        {
            get { return _dataFeatures.Value; }
        }
        private ISecurityService SecurityService
        {
            get { return _securityService.Value; }
        }
        private ISchemaService SchemaService
        {
            get { return _schemaService.Value; }
        }
        private IJobService JobService
        {
            get { return _jobService.Value; }
        }
        private IQuartermasterService QuartermasterService
        {
            get { return _quartermasterService.Value; }
        }
        #endregion

        #region Public Methods        
        public bool DeleteDataset(List<int> deleteIdList, IApplicationUser user, bool forceDelete = false)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(DeleteDataset).ToLower()}";
            Logger.Info($"{methodName} Method Start");

            bool result = Delete(deleteIdList, user, DatasetService, forceDelete);

            Logger.Info($"{methodName} Method End");
            return result;
        }

        public bool DeleteDatasetFileConfig(List<int> deleteIdList, IApplicationUser user, bool forceDelete = false)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(DeleteDatasetFileConfig).ToLower()}";
            Logger.Info($"{methodName} Method Start");

            bool result = Delete(deleteIdList, user, ConfigService, forceDelete);

            Logger.Info($"{methodName} Method End");
            return result;
        }

        public bool DeleteDataflow(List<int> deleteIdList, IApplicationUser user, bool forceDelete = false)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(DeleteDataflow).ToLower()}";
            Logger.Info($"{methodName} Method Start");

            bool result = Delete(deleteIdList, user, DataFlowService, forceDelete);

            Logger.Info($"{methodName} Method End");
            return result;
        }

        public bool DeleteDataflow(List<int> deleteIdList, string userId, bool forceDelete = false)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(Delete).ToLower()}";
            Logger.Info($"{methodName} Method Start");

            //get user
            IApplicationUser user = UserService.GetByAssociateId(userId);

            //pass to private delete
            bool isSuccessful = Delete(deleteIdList, user, DataFlowService, forceDelete);

            Logger.Info($"{methodName} Method End");
            return isSuccessful;
        }

        public bool DeleteDataFlow_Queue(List<int> deleteIdList, string userId, bool forceDelete = false)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(DeleteDataFlow_Queue).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            bool successfullySubmitted = true;
            try
            {
                foreach (int id in deleteIdList)
                {
                    BackgroundJobClient.Enqueue<DataApplicationService>(x => x.DeleteDataflow(new List<int> { id }, userId, true));
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{methodName} hangfire submission failure", ex);
                successfullySubmitted = false;
            }

            Logger.Info($"{methodName} Method End");
            return successfullySubmitted;
        }


        public async Task<DatasetMigrationRequestResponse> MigrateDataset(DatasetMigrationRequest migrationRequest)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(MigrateDataset).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            Logger.Info($"{methodName} {JsonConvert.SerializeObject(migrationRequest)}");
            
            //Perform validations on incoming request object
            List<string> errors = await ValidateMigrationRequest(migrationRequest);
            if (errors.Any())
            {
                List<ArgumentException> exceptions = new List<ArgumentException>();
                foreach (string error in errors)
                {
                    exceptions.Add(new ArgumentException(error));
                }
                throw new AggregateException(exceptions);
            }

            DatasetMigrationRequestResponse response = new DatasetMigrationRequestResponse();
            try
            {
                Tuple<string, string, string> sourceDatasetMetadata = _datasetContext.Datasets.Where(w => w.DatasetId == migrationRequest.SourceDatasetId).Select(s => new Tuple<string, string, string>(s.DatasetName, s.Asset.SaidKeyCode, s.NamedEnvironment)).FirstOrDefault();
                if (sourceDatasetMetadata == null)
                {
                    throw new DatasetNotFoundException("Source dataset not found");
                }

                //Do not perform dataset metadata migration if it already exists in target environment,
                //  instead grab datasetId of existing target dataset.
                (int targetDatasetId, bool datasetExistsInTarget) = DatasetService.DatasetExistsInTargetNamedEnvironment(sourceDatasetMetadata.Item1, sourceDatasetMetadata.Item2, migrationRequest.TargetDatasetNamedEnvironment);

                //Check user permisions against target if it exists else check against source
                CheckPermissionToMigrateDataset(datasetExistsInTarget ? targetDatasetId : migrationRequest.SourceDatasetId);

                int newDatasetId;
                if (!datasetExistsInTarget)
                {
                    DatasetDto dto = DatasetService.GetDatasetDto(migrationRequest.SourceDatasetId);

                    dto.DatasetId = 0;
                    dto.NamedEnvironment = migrationRequest.TargetDatasetNamedEnvironment;

                    newDatasetId = CreateWithoutSave(dto);

                    if (newDatasetId == 0)
                    {
                        throw new DataApplicationServiceException("Failed to create dataset metadata");
                    }
                }
                else
                {
                    newDatasetId = targetDatasetId;
                }

                migrationRequest.TargetDatasetId = newDatasetId;
                migrationRequest.SchemaMigrationRequests.ForEach(i => i.TargetDatasetId = newDatasetId);

                //If user has permissions to migrate dataset, they automatically have permissions to migrate schema
                List<SchemaMigrationRequestResponse> schemaMigrationResponses = MigrateSchemaWithoutSave_Internal(migrationRequest.SchemaMigrationRequests);

                //All entity objects have been created, therefore, save changes
                _datasetContext.SaveChanges();

                //Only create dataset external dependencies if this migration created the dataset
                if (!datasetExistsInTarget)
                {
                    //Create dataset external dependencies
                    CreateExternalDependenciesForDataset(new List<int>() { newDatasetId });
                }

                //Only kick off dataflow external dependencies if the dataflow metadata was migrated
                var schemaWithMigratedDataflows = schemaMigrationResponses.Where(w => w.MigratedDataFlow).ToList();
                List<int> newSchemaIds = Enumerable.Range(0, schemaWithMigratedDataflows.Count).Select(i => schemaMigrationResponses[i].TargetSchemaId).ToList();
                CreateExternalDependenciesForDataFlowBySchemaId(newSchemaIds);

                //Only kick off schema revision external dependecies if the schema revision was migrated
                var migratedSchemaRevisions = schemaMigrationResponses.Where(w => w.MigratedSchemaRevision).Select(s => (s.TargetSchemaId, s.TargetSchemaRevisionId)).ToList();
                CreateExternalDependenciesForSchemaRevision(migratedSchemaRevisions);

                response.IsDatasetMigrated = true;
                response.DatasetMigrationReason = "Success";
                response.DatasetId = newDatasetId;
                response.SchemaMigrationResponses = schemaMigrationResponses;
            }
            catch (Exception ex)
            {
                Logger.Error($"{methodName} Failed to perform migration", ex);
                _datasetContext.Clear();
                throw;
            }

            Logger.Info($"{methodName} Method End");
            return response;
        }

        public SchemaMigrationRequestResponse MigrateSchema(SchemaMigrationRequest request)
        {
            return MigrateSchema_Internal(request);
        }
        #endregion

        #region Private methods        
        /// <summary>
        /// Creates new dataset entity object and saves domain context.
        /// <para>Also calls <see cref="DatasetService.CreateExternalDependencies(int)"></see> to create any external dependencies.</para>
        /// </summary>
        /// <param name="dto">DatasetDto</param>
        /// <returns>Id of new dataset object.</returns>
        internal int Create(DatasetDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(Create).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            int newDatasetId;
            try
            {
                newDatasetId = CreateWithoutSave(dto);
                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to save Dataset", ex);
                _datasetContext.Clear();
                throw;
            }

            try
            {
                CreateExternalDependenciesForDataset(new List<int>() { newDatasetId });
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to create external dependencies for dataflow", ex);
            }

            Logger.Info($"{methodName} Method End");
            return newDatasetId;
        }
        /// <summary>
        /// Creates new datasetfileconfig entity object and saves to domain context.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>Id of new datasetfileconfig object.</returns>
        internal int Create(DatasetFileConfigDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(Create).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            int newDatasetFileConfigId;
            try
            {
                newDatasetFileConfigId = CreateWithoutSave(dto);
                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to save DatasetFileConfig", ex);
                _datasetContext.Clear();
                throw;
            }

            Logger.Info($"{methodName} Method End");
            return newDatasetFileConfigId;
        }
        /// <summary>
        /// Creates new dataflow entity object ans saves to domain context.  
        /// <para>Also calls <see cref="DataFlowService.CreateExternalDependencies(int)"></see> to create any external dependencies.</para>
        /// </summary>
        /// <param name="dto">DataFlowDtoDto</param>
        /// <returns>Id of new dataflow object.</returns>
        internal int Create(DataFlowDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(Create).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            int newDataFlowId;
            try
            {
                newDataFlowId = CreateWithoutSave(dto);
                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to save DataFlow", ex);
                _datasetContext.Clear();
                throw;
            }

            try
            {
                CreateExternalDependenciesForDataFlow(new List<int>() { newDataFlowId });
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to create external dependencies for dataflow", ex);
            }


            Logger.Info($"{methodName} Method End");
            return newDataFlowId;
        }
        /// <summary>
        /// Creates new fileschema entity object and saves to domain context.
        /// </summary>
        /// <param name="dto">FileSchemaDto</param>
        /// <returns>Id of new fileschema object.</returns>
        internal int Create(FileSchemaDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(Create).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            int newFileSchemaId;
            try
            {
                newFileSchemaId = CreateWithoutSave(dto);
                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to save FileSchema", ex);
                _datasetContext.Clear();
                throw;
            }

            Logger.Info($"{methodName} Method End");
            return newFileSchemaId;
        }

        /// <summary>
        ///  Creates new dataset entity object.    
        /// <para>Caller is responsible for saving to domain context and calling <see cref="DatasetService.CreateExternalDependencies(int)"></see> to create any external dependencies.</para>
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>Id of new dataset object.</returns>
        internal virtual int CreateWithoutSave(DatasetDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(CreateWithoutSave).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            int newDatasetId;
            try
            {
                newDatasetId = DatasetService.Create(dto);
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to create Dataset", ex);
                throw;
            }

            Logger.Info($"{methodName} Method End");
            return newDatasetId;
        }
        /// <summary>
        ///  Creates new datasetfileconfig entity object.    
        /// <para>Caller is responsible for saving to domain context.</para>
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>Id of new datasetfileconfig object.</returns>
        internal virtual int CreateWithoutSave(DatasetFileConfigDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(CreateWithoutSave).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            int newConfigId;
            try
            {
                newConfigId = ConfigService.Create(dto);
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to create DatasetFileConfig", ex);
                throw;
            }

            Logger.Info($"{methodName} Method End");
            return newConfigId;
        }
        /// <summary>
        /// Creates new dataflow entity object and saves domain context.
        /// <para>Also calls <see cref="DataFlowService.CreateExternalDependencies(int)"></see> to create any external dependencies.</para>
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>Id of new dataflow object.</returns>
        internal virtual int CreateWithoutSave(DataFlowDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(CreateWithoutSave).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            DataFlow newDataFlow;
            try
            {
                //Create dataflow / dataflowstep entities
                newDataFlow = DataFlowService.Create(dto);

                switch (dto.IngestionType)
                {
                    case (int)IngestionType.DFS_Drop:
                    case (int)IngestionType.S3_Drop:
                    case (int)IngestionType.Topic:
                        CreateWithoutSave_DfsRetrieverJob(newDataFlow);
                        break;
                    case (int)IngestionType.DSC_Pull:
                        newDataFlow.MapToRetrieverJobDto(dto.RetrieverJob);
                        CreateWithoutSave_ExternalRetrieverJob(dto.RetrieverJob);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to create DataFlow", ex);
                throw;
            }
            Logger.Info($"{methodName} Method End");
            return newDataFlow.Id;
        }
        /// <summary>
        /// Creates new fileschema entity object and saves to domain context.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>Id of new fileschema object.</returns>
        internal virtual int CreateWithoutSave(FileSchemaDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(CreateWithoutSave).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            int newFileSchemaId;
            try
            {
                newFileSchemaId = SchemaService.Create(dto);
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to create FileSchema", ex);
                throw;
            }

            Logger.Info($"{methodName} Method End");
            return newFileSchemaId;
        }
        private void CreateWithoutSave_DfsRetrieverJob(DataFlow dataFlow)
        {
            if (dataFlow.ShouldCreateDFSDropLocations(DataFeatures))
            {
                JobService.CreateDfsRetrieverJob(dataFlow);
            }
        }
        private void CreateWithoutSave_ExternalRetrieverJob(RetrieverJobDto retrieverJobDto)
        {
            _ = JobService.CreateRetrieverJob(retrieverJobDto);
        }

        internal virtual int CreateWithoutSave(SchemaRevisionFieldStructureDto dto)
        {
            int newRevisionId = SchemaService.CreateSchemaRevision(dto);
            return newRevisionId;
        }


        /// <summary>
        /// Migrates list of schema without saving.
        /// <para>Retuns a list of Tuple<int, int> consisting of schemaId, , schemaRevisionId pairs</para>
        /// </summary>
        /// <param name="requestList"></param>
        /// <returns></returns>
        /// <exception cref="DataApplicationServiceException"></exception>
        internal virtual List<SchemaMigrationRequestResponse> MigrateSchemaWithoutSave_Internal(List<SchemaMigrationRequest> requestList)
        {
            List<SchemaMigrationRequestResponse> responseList = new List<SchemaMigrationRequestResponse> ();
            foreach (SchemaMigrationRequest schemaMigration in requestList)
            {
                SchemaMigrationRequestResponse response = MigrateSchemaWithoutSave_Internal(schemaMigration);
                responseList.Add(response);
            }
            return responseList;
        }

        /// <summary>
        /// Migrates schema without saving.
        /// <para>Retuns a Tuple<int, int> consisting of schemaId, schemaRevisionId</para>
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="AggregateException"></exception>
        /// <returns>Tuple<int, int></returns>
        internal SchemaMigrationRequestResponse MigrateSchemaWithoutSave_Internal(SchemaMigrationRequest request)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(MigrateSchemaWithoutSave_Internal).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            Logger.Info($"{methodName} Processing : {JsonConvert.SerializeObject(request)}");

            SchemaMigrationRequestResponse migrationResponse =  new SchemaMigrationRequestResponse();

            ValidateMigrationRequest(request);

            int newSchemaId;
            int newSchemaRevisionId;
            (int existingTargetSchemaId, bool schemaExistsInTargetDataset) targetSchemaMetadata;
            try
            {
                string sourceSchemaName = _datasetContext.FileSchema.Where(w => w.SchemaId == request.SourceSchemaId).Select(s => s.Name).FirstOrDefault();
                targetSchemaMetadata = SchemaService.SchemaExistsInTargetDataset(request.TargetDatasetId, sourceSchemaName);

                int sourceDatasetId = (_datasetContext.DatasetFileConfigs.Any(w => w.Schema.SchemaId == request.SourceSchemaId && w.ObjectStatus == ObjectStatusEnum.Active))
                                            ? _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == request.SourceSchemaId && w.ObjectStatus == ObjectStatusEnum.Active).Select(s => s.ParentDataset.DatasetId).FirstOrDefault()
                                            : 0;

                CheckPermissionToMigrateSchema(targetSchemaMetadata.schemaExistsInTargetDataset ? targetSchemaMetadata.existingTargetSchemaId : request.SourceSchemaId);

                if (!targetSchemaMetadata.schemaExistsInTargetDataset)
                {
                    //Retrieve source dto objects
                    FileSchemaDto schemaDto = SchemaService.GetFileSchemaDto(request.SourceSchemaId);

                    //Adjust schemaDto associations and create new entity
                    schemaDto.ParentDatasetId = request.TargetDatasetId;
                    newSchemaId = CreateWithoutSave(schemaDto);

                    DatasetFileConfigDto configDto = ConfigService.GetDatasetFileConfigDtoByDataset(sourceDatasetId).FirstOrDefault(x => x.Schema.SchemaId == request.SourceSchemaId);

                    //Adjust datasetFileConfigDto associations and create new entity
                    configDto.SchemaId = newSchemaId;
                    configDto.ParentDatasetId = request.TargetDatasetId;
                    _ = CreateWithoutSave(configDto);

                    migrationResponse.MigratedSchema = true;
                    migrationResponse.TargetSchemaId = newSchemaId;
                    migrationResponse.SchemaMigrationReason = "Success";
                }
                else
                {
                    newSchemaId = targetSchemaMetadata.existingTargetSchemaId;
                    migrationResponse.TargetSchemaId = newSchemaId;
                    migrationResponse.SchemaMigrationReason = "Schema configuration existed in target";
                }

                newSchemaRevisionId = MigrateSchemaRevisionWithoutSave_Internal(request.SourceSchemaId, sourceDatasetId, newSchemaId);

                if (newSchemaRevisionId != 0)
                {
                    migrationResponse.MigratedSchemaRevision = true;
                    migrationResponse.TargetSchemaRevisionId = newSchemaRevisionId;
                    migrationResponse.SchemaRevisionMigrationReason = "Success";
                }
                else
                {
                    migrationResponse.MigratedSchemaRevision = false;
                    migrationResponse.SchemaRevisionMigrationReason = "No column metadata on source schema";
                }

                (int newDataFlowId, bool wasDataFlowMigrated) = MigrateDataFlowWihtoutSave_Internal(newSchemaId, request.SourceSchemaId, request.SourceSchemaHasDataFlow, sourceDatasetId);
                
                if (wasDataFlowMigrated)
                {
                    migrationResponse.MigratedDataFlow = true;
                    migrationResponse.TargetDataFlowId = newDataFlowId;
                    migrationResponse.DataFlowMigrationReason = "Success";
                }
                else
                {
                    string message = (request.SourceSchemaHasDataFlow) ? "Target dataflow metadata already exists" : "Source schema is not associated with dataflow";
                    migrationResponse.DataFlowMigrationReason = message;
                    migrationResponse.TargetDataFlowId = newDataFlowId;
                }
            }
            catch (SchemaUnauthorizedAccessException)
            {
                Logger.Info($"User unauthorized to migrate schema");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"{methodName} - Failed to migrate schema.", ex);
                throw;
            }

            Logger.Info($"{methodName} Method End");

            return migrationResponse;
        }

        private (int newDataFlowId, bool wasDataFlowMigrated) MigrateDataFlowWihtoutSave_Internal(int newSchemaId, int sourceSchemaId, bool sourceSchemaHasDataFlow, int targetDatasetId)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(MigrateSchemaWithoutSave_Internal).ToLower()}";
            Logger.Info($"{methodName} Method Start");

            //Do not migrate DataFlow configuration if already exists in target
            int targetDataFlowId = 0;
            bool migratedDataflow = false;

            int existingTargetDataflowId = _datasetContext.DataFlow.Where(w => w.SchemaId == newSchemaId && (w.ObjectStatus == ObjectStatusEnum.Active || w.ObjectStatus == ObjectStatusEnum.Disabled)).Select(s => s.Id).FirstOrDefault();
            bool targetSchemaHasDataFlow = (existingTargetDataflowId != 0);
            
            if (sourceSchemaHasDataFlow && !targetSchemaHasDataFlow)
            {
                //Determeine if source schema has dataflow, if so perform dataflow migration as well                
                DataFlowDetailDto dataflowDto = DataFlowService.GetDataFlowDetailDtoBySchemaId(sourceSchemaId).FirstOrDefault();

                //Adjust dataFlowDto associations and create new entity
                dataflowDto.DatasetId = targetDatasetId;
                dataflowDto.SchemaId = newSchemaId;
                dataflowDto.SchemaMap.First().SchemaId = newSchemaId;
                dataflowDto.SchemaMap.First().DatasetId = targetDatasetId;
                dataflowDto.SchemaMap.First().StepId = 0;

                targetDataFlowId = CreateWithoutSave(dataflowDto);
                migratedDataflow = true;
            }
            else
            {
                targetDataFlowId = existingTargetDataflowId;
                string message = (sourceSchemaHasDataFlow) ? "Target dataflow metadata already exists" : "Source schema is not associcated with dataflow";
                Logger.Info($"{methodName} {message}");
            }

            Logger.Info($"{methodName} Method End");
            return (targetDataFlowId, migratedDataflow);
        }

        private int MigrateSchemaRevisionWithoutSave_Internal(int sourceSchemaId, int sourceDatasetId, int targetSchemaId)
        {
            int newSchemaRevisionId = 0;
            SchemaRevisionFieldStructureDto schemaRevisionFieldStructureDto = SchemaService.GetLatestSchemaRevisionFieldStructureBySchemaId(sourceDatasetId, sourceSchemaId);
            if (schemaRevisionFieldStructureDto.Revision != null)
            {
                schemaRevisionFieldStructureDto.Revision.SchemaId = targetSchemaId;
                schemaRevisionFieldStructureDto.Revision.SchemaRevisionName = $"Migration_{DateTime.Now:yyyyMMdd-HHmmss}";

                newSchemaRevisionId = CreateWithoutSave(schemaRevisionFieldStructureDto);
            }

            return newSchemaRevisionId;
        }

        internal SchemaMigrationRequestResponse MigrateSchema_Internal(SchemaMigrationRequest migrationRequest)
        {
            SchemaMigrationRequestResponse response = MigrateSchemaWithoutSave_Internal(migrationRequest);
            _datasetContext.SaveChanges();

            if (response.MigratedDataFlow)
            {
                CreateExternalDependenciesForDataFlowBySchemaId(new List<int>() { response.TargetSchemaId });
            }

            if (response.MigratedSchemaRevision)
            {
                CreateExternalDependenciesForSchemaRevision(new List<(int, int)> { (response.TargetSchemaId, response.TargetSchemaRevisionId) });
            }

            return response;
        }


        internal virtual void CreateExternalDependenciesForDataset(List<int> datasetIdList)
        {
            foreach (int datasetId in datasetIdList)
            {
                DatasetService.CreateExternalDependencies(datasetId);
            }
        }

        internal virtual void CreateExternalDependenciesForSchemaRevision(List<(int schemaId, int schemaRevisionId)> schemaAndSchemaRevisionIdList)
        {
            foreach ((int schemaId, int schemaRevisionId) in schemaAndSchemaRevisionIdList)
            {
                CreateExternalDependenciesForSchemaRevision(schemaId, schemaRevisionId);
            }
        }
        private void CreateExternalDependenciesForSchemaRevision(int schemaId, int schemaRevisionId)
        {
            SchemaService.CreateSchemaRevisionExternalDependencies(schemaId, schemaRevisionId);
        }

        internal virtual void CreateExternalDependenciesForDataFlow(List<int> dataFlowIdList)
        {
            foreach (int dataFlowId in dataFlowIdList)
            {
                DataFlowService.CreateExternalDependencies(dataFlowId);
            }
        }
        internal virtual void CreateExternalDependenciesForDataFlowBySchemaId(List<int> schemaIdList)
        {
            foreach (int schemaId in schemaIdList)
            {
                int dataFlowId = _datasetContext.DataFlow.FirstOrDefault(w => w.SchemaId == schemaId && w.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active).Id;
                DataFlowService.CreateExternalDependencies(dataFlowId);
            }
        }

        /// <summary>
        /// Validates schema migration request
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="AggregateException">Aggregation of all validation errors</exception>
        private void ValidateMigrationRequest(SchemaMigrationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException($"{nameof(request)}");
            }

            List<string> errors = new List<string>();
            int sourceDatasetId = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == request.SourceSchemaId).Select(w => w.ParentDataset.DatasetId).FirstOrDefault();
            bool sourceSchemaHasDataflow = _datasetContext.DataFlow.Where(w => w.SchemaId == request.SourceSchemaId && (w.ObjectStatus == ObjectStatusEnum.Disabled || w.ObjectStatus == ObjectStatusEnum.Active)).Any();
            
            if (request.SourceSchemaId == 0)
            {
                errors.Add("SourceSchemaId is required");
            }

            if (sourceSchemaHasDataflow && string.IsNullOrWhiteSpace(request.TargetDataFlowNamedEnvironment))
            {
                errors.Add("TargetDataFlowNamedEnvironment is required");
            } 

            if (!AreDatasetsRelated(sourceDatasetId, request.TargetDatasetId))
            {
                errors.Add("Source and target datasets are not related");
            }

            ValidateMigrationRequest((MigrationRequest)request).ForEach(i => errors.Add(i));

            if (errors.Any())
            {
                List<ArgumentException> exceptions = new List<ArgumentException>();
                foreach (string error in errors)
                {
                    exceptions.Add(new ArgumentException(error));
                }
                throw new AggregateException(exceptions);
            }
        }

        private Task<List<string>> ValidateMigrationRequest(DatasetMigrationRequest request)
        {
            List<string> errors = new List<string>();
            if (request == null)
            {
                throw new ArgumentNullException($"{nameof(request)}");
            }

            if (request.SourceDatasetId == 0)
            {
                errors.Add("SourceDatasetId is required");
            }

            if (request.TargetDatasetId != 0 && !AreDatasetsRelated(request.SourceDatasetId, request.TargetDatasetId))
            {
                errors.Add("Source and target datasets are not related");
            }

            ValidateMigrationRequest((MigrationRequest)request).ForEach(i => errors.Add(i));

            // This "wrapping" of the async portion of this method is required so that the error checking above runs immediately.
            // See https://sonarqube.sentry.com/coding_rules?open=csharpsquid%3AS4457&rule_key=csharpsquid%3AS4457
            return ValidateMigrationRequestAsync();            
            async Task<List<string>> ValidateMigrationRequestAsync()
            {
                if (request.TargetDatasetId == 0 && !await IsNamedEnvironmentRelatedToSaidAsset(request.SourceDatasetId, request.TargetDatasetNamedEnvironment))
                {
                    errors.Add("Target named environment is not related to SAID asset associated with target dataset");
                }

                return errors;
            }            
        }

        private List<string> ValidateMigrationRequest(MigrationRequest request)
        {
            List<string> errors = new List<string>();
            if (request == null)
            {
                throw new ArgumentNullException($"{nameof(request)}");
            }

            if (request.TargetDatasetId < 0)
            {
                errors.Add("TargetDatasetId cannot be a negative number");
            }

            if (String.IsNullOrWhiteSpace(request.TargetDatasetNamedEnvironment))
            {
                errors.Add("TargetDatasetNamedEnvironment is required");
            }

            return errors;
        }

        internal async Task<bool> IsNamedEnvironmentRelatedToSaidAsset(int datasetId, string namedEnvironment)
        {
            bool IsRelated = true;

            (string datasetSaidAssetKeyCode, NamedEnvironmentType datasetNamedEnvironmentType) = _datasetContext.Datasets.Where(w => w.DatasetId == datasetId).Select(s => new Tuple<string, NamedEnvironmentType>(s.Asset.SaidKeyCode, s.NamedEnvironmentType)).FirstOrDefault();
            
            var results = await QuartermasterService.VerifyNamedEnvironmentAsync(namedEnvironment, datasetSaidAssetKeyCode, datasetNamedEnvironmentType);

            if (results != null && !results.IsValid())
            {
                IsRelated = false;
            }

            return IsRelated;

        }

        internal bool AreDatasetsRelated(int firstDatasetId, int secondDatasetId)
        {
            //What equates to related datasets
            //  1.) Dataset names are the same
            //  2.) SAID asset codes are the same
            Tuple<string, string> firstDataset = _datasetContext.Datasets.Where(w => w.DatasetId == firstDatasetId).Select(s => new Tuple<string, string>(s.DatasetName, s.Asset.SaidKeyCode)).FirstOrDefault();
            Tuple<string, string> secondDataset = _datasetContext.Datasets.Where(w => w.DatasetId == secondDatasetId).Select(s => new Tuple<string, string>(s.DatasetName, s.Asset.SaidKeyCode)).FirstOrDefault();
            
            if(firstDataset == null || secondDataset == null)
            {
                return false;
            }

            return firstDataset.Equals(secondDataset);
        }

        private void CheckPermissionToMigrateDataset(int datasetId)
        {
            //Check if user has permissions to migrate
            IApplicationUser user = UserService.GetCurrentUser();
            UserSecurity userSecurity = SecurityService.GetUserSecurity(_datasetContext.GetById<Dataset>(datasetId), user);
            if (!userSecurity.CanEditDataset)
            {
                throw new DatasetUnauthorizedAccessException();
            }
        }

        internal virtual void CheckPermissionToMigrateSchema(int schemaId)
        {
            //Check if user has permissions to migrate
            IApplicationUser user = UserService.GetCurrentUser();
            UserSecurity userSecurity = SecurityService.GetUserSecurity(_datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schemaId).Select(s => s.ParentDataset).FirstOrDefault(), user);
            if (!userSecurity.CanManageSchema)
            {
                throw new SchemaUnauthorizedAccessException();
            }
        }

        private bool Delete<T>(List<int> deleteIdList, IApplicationUser user, T instance, bool forceDelete = false) where T : IEntityService
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(Delete).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            bool IsSuccessful = true;

            try
            {                
                foreach (int id in deleteIdList)
                {
                    bool singleResult = instance.Delete(id, user, !forceDelete);

                    if (!singleResult)
                    {
                        IsSuccessful = singleResult;
                    }
                }

                if (IsSuccessful)
                {
                    _datasetContext.SaveChanges();
                }
                else
                {
                    Logger.Warn($"Delete failed for Ids :{string.Join(":::", deleteIdList)}");
                    _datasetContext.Clear();
                }
            }
            catch(Exception Ex)
            {
                Logger.Error($"Delete Failed", Ex);
                _datasetContext.Clear();
                IsSuccessful = false;
            }

            Logger.Info($"{methodName} Method End");
            return IsSuccessful;
        }
        #endregion
    }
}
