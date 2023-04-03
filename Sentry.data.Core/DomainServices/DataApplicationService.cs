using Hangfire;
using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Extensions;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

            if (!DataFeatures.CLA1797_DatasetSchemaMigration.GetValue())
            {
                throw new FeatureNotEnabledException("Migration feature is not enabled.");
            }
            
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

                //If user has permissions to migrate dataset, they automatically have permissions to migrate schema
                List<SchemaMigrationRequestResponse> schemaMigrationResponses = MigrateSchemaWithoutSave_Internal(migrationRequest);

                //All entity objects have been created, therefore, save changes
                _datasetContext.SaveChanges();

                response.IsDatasetMigrated = !datasetExistsInTarget;
                response.DatasetMigrationReason = datasetExistsInTarget ? "Dataset already exists in target named environment" : "Success";
                response.DatasetId = newDatasetId;
                response.DatasetName = sourceDatasetMetadata.Item1;
                response.SchemaMigrationResponses = schemaMigrationResponses;

                try
                {                    
                    //Only create dataset external dependencies if this migration created the dataset
                    if (!datasetExistsInTarget)
                    {
                        //Create dataset external dependencies
                        CreateExternalDependenciesForDataset(new List<int>() { newDatasetId });
                    }

                    var migratedSchemaIds = schemaMigrationResponses.Where(x => x.MigratedSchema).Select(x => x.TargetSchemaId).ToList();
                    CreateExternalDependenciesForSchemas(migratedSchemaIds);

                    //Only kick off dataflow external dependencies if the dataflow metadata was migrated
                    var schemaWithMigratedDataflows = schemaMigrationResponses.Where(w => w.MigratedDataFlow).ToList();
                    List<int> newSchemaIds = Enumerable.Range(0, schemaWithMigratedDataflows.Count).Select(i => schemaMigrationResponses[i].TargetSchemaId).ToList();
                    CreateExternalDependenciesForDataFlowBySchemaId(newSchemaIds);
                    
                    //Only kick off schema revision external dependecies if the schema revision was migrated
                    var migratedSchemaRevisions = schemaMigrationResponses.Where(w => w.MigratedSchemaRevision).Select(s => (s.TargetSchemaId, s.TargetSchemaRevisionId)).ToList();
                    CreateExternalDependenciesForSchemaRevision(migratedSchemaRevisions);
                }
                catch (Exception ex)
                {
                    Logger.Error($"{methodName} - Failed creating external dependencies", ex);

                    // Rollback newly created objects
                    Logger.Info($"{methodName} - Rollback initiated");
                    RollbackDatasetMigration(response);
                    _datasetContext.SaveChanges();
                    Logger.Info($"{methodName} - Rollback completed");
                    throw;
                }

                CreateMigrationHistory(migrationRequest, response);
                _datasetContext.SaveChanges();
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

        //create a method to log all the migration history
        internal virtual void CreateMigrationHistory(DatasetMigrationRequest request, DatasetMigrationRequestResponse response)
        {
            MigrationHistory history = AddMigrationHistory(request, response);
            AddMigrationHistoryDetailDataset(history, request, response);
            AddMigrationHistoryDetailSchemas(history, request, response);
        }

        internal MigrationHistory AddMigrationHistory(DatasetMigrationRequest request, DatasetMigrationRequestResponse response)
        {
            MigrationHistory history = null;
            try
            {
                int? myNullInt = null;
                history = new MigrationHistory()
                {
                    CreateDateTime = DateTime.Now,
                    TargetNamedEnvironment = request.TargetDatasetNamedEnvironment,
                    SourceDatasetId = request.SourceDatasetId,
                    TargetDatasetId = (request.TargetDatasetId == 0) ? myNullInt : request.TargetDatasetId
                };

                Dataset dataset = _datasetContext.Datasets.FirstOrDefault(w => w.DatasetId == request.SourceDatasetId);
                if (dataset != null)
                {
                    history.SourceNamedEnvironment = dataset.NamedEnvironment;
                }

                _datasetContext.Add(history);
                
            }
            catch (Exception ex)
            {
                Logger.Error($"{nameof(AddMigrationHistory)} Failed to migrate history", ex);
                _datasetContext.Clear();
                throw;
            }
            return history;
        }

        internal void AddMigrationHistoryDetailDataset(MigrationHistory history, DatasetMigrationRequest request, DatasetMigrationRequestResponse response)
        {
            int? myNullInt = null;
            try
            {
                //CREATE DATASET HISTORY
                MigrationHistoryDetail historyDataset = new MigrationHistoryDetail()
                {
                    MigrationHistoryId = history.MigrationHistoryId,
                    SourceDatasetId = request.SourceDatasetId,
                    IsDatasetMigrated = response.IsDatasetMigrated,
                    DatasetId = (response.DatasetId == 0) ? myNullInt : response.DatasetId,
                    DatasetName = response.DatasetName,
                    DatasetMigrationMessage = response.DatasetMigrationReason,
                    SchemaId = myNullInt,
                    DataFlowId = myNullInt,
                    SchemaRevisionId = myNullInt,
                    SourceSchemaId = myNullInt
                };
                _datasetContext.Add(historyDataset);
                
            }
            catch (Exception ex)
            {
                Logger.Error($"{nameof(AddMigrationHistoryDetailDataset)} Failed to migrate history", ex);
                _datasetContext.Clear();
                throw;
            }
        }

        internal void AddMigrationHistoryDetailSchemas(MigrationHistory history, DatasetMigrationRequest request, DatasetMigrationRequestResponse response)
        {
            int? myNullInt = null;
            try
            {
                //LOOP THROUGH EACH SCHEMA AND ADD SCHEMA HISTORY
                foreach (SchemaMigrationRequestResponse schema in response.SchemaMigrationResponses)
                {
                    MigrationHistoryDetail historySchema = new MigrationHistoryDetail()
                    {
                        MigrationHistoryId = history.MigrationHistoryId,
                        DatasetId = myNullInt,
                        SourceDatasetId = request.SourceDatasetId,

                        //SCHEMA
                        SourceSchemaId = schema.SourceSchemaId,
                        IsSchemaMigrated = schema.MigratedSchema,
                        SchemaId = (schema.TargetSchemaId == 0) ? myNullInt : schema.TargetSchemaId,
                        SchemaName = schema.SchemaName,
                        SchemaMigrationMessage = schema.SchemaMigrationReason,

                        //DATAFLOW
                        IsDataFlowMigrated = schema.MigratedDataFlow,
                        DataFlowId = (schema.TargetDataFlowId == 0) ? myNullInt : schema.TargetDataFlowId,
                        DataFlowName = schema.DataFlowName,
                        DataFlowMigrationMessage = schema.DataFlowMigrationReason,

                        //SCHEMA REVISION
                        IsSchemaRevisionMigrated = schema.MigratedSchemaRevision,
                        SchemaRevisionId = (schema.TargetSchemaRevisionId == 0) ? myNullInt : schema.TargetSchemaRevisionId,
                        SchemaRevisionName = schema.SchemaRevisionName,
                        SchemaRevisionMigrationMessage = schema.SchemaRevisionMigrationReason
                    };
                    _datasetContext.Add(historySchema);
                }
                
            }
            catch (Exception ex)
            {
                Logger.Error($"{nameof(AddMigrationHistoryDetailSchemas)} Failed to migrate history", ex);
                _datasetContext.Clear();
                throw;
            }
        }

        public SchemaMigrationRequestResponse MigrateSchema(SchemaMigrationRequest request)
        {
            if (!DataFeatures.CLA1797_DatasetSchemaMigration.GetValue())
            {
                throw new UnauthorizedAccessException("Not authorized to use this functionality");
            }

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
        internal virtual int CreateWithoutSave(SchemaRevisionFieldStructureDto dto)
        {
            return SchemaService.CreateSchemaRevision(dto);
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

        public List<SchemaMigrationRequestResponse> MigrateSchemaWithoutSave_Internal(DatasetMigrationRequest request)
        {
            List<SchemaMigrationRequest> schemaRequests = request.MapToSchemaMigrationRequest();
            return MigrateSchemaWithoutSave_Internal(schemaRequests);
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
            migrationResponse.SourceSchemaId = request.SourceSchemaId;      //add sourceSchemaId because migration history will need it and its easier to decorate here then figure it out on backend

            bool sourceHasDataFlow = _datasetContext.DataFlow.Where(w => w.SchemaId == request.SourceSchemaId && (w.ObjectStatus == ObjectStatusEnum.Disabled || w.ObjectStatus == ObjectStatusEnum.Active)).Any();

            List<string> errors = ValidateMigrationRequest(request, sourceHasDataFlow);
            if (errors.Any())
            {
                List<ArgumentException> exceptions = new List<ArgumentException>();
                foreach (string error in errors)
                {
                    exceptions.Add(new ArgumentException(error));
                }
                throw new AggregateException(exceptions);
            }

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
                    migrationResponse.SchemaName = sourceSchemaName;
                    migrationResponse.SchemaMigrationReason = "Success";
                }
                else
                {
                    newSchemaId = targetSchemaMetadata.existingTargetSchemaId;
                    migrationResponse.TargetSchemaId = newSchemaId;
                    migrationResponse.SchemaName = sourceSchemaName;
                    migrationResponse.SchemaMigrationReason = "Schema configuration existed in target";
                }

                newSchemaRevisionId = MigrateSchemaRevisionWithoutSave_Internal(request.SourceSchemaId, sourceDatasetId, newSchemaId);

                if (newSchemaRevisionId != 0)
                {
                    migrationResponse.MigratedSchemaRevision = true;
                    migrationResponse.TargetSchemaRevisionId = newSchemaRevisionId;
                    migrationResponse.SchemaRevisionName = _datasetContext.SchemaRevision.Where(w => w.SchemaRevision_Id == newSchemaRevisionId).Select(s => s.SchemaRevision_Name).FirstOrDefault();
                    migrationResponse.SchemaRevisionMigrationReason = "Success";
                }
                else
                {
                    migrationResponse.MigratedSchemaRevision = false;
                    migrationResponse.SchemaRevisionMigrationReason = "No column metadata on source schema";
                }

                (int newDataFlowId, bool wasDataFlowMigrated) = MigrateDataFlowWihtoutSave_Internal(newSchemaId, request.SourceSchemaId, sourceHasDataFlow, request.TargetDatasetId);
                
                if (wasDataFlowMigrated)
                {
                    migrationResponse.MigratedDataFlow = true;
                    migrationResponse.TargetDataFlowId = newDataFlowId;
                    migrationResponse.DataFlowName = _datasetContext.DataFlow.Where(w => w.Id == newDataFlowId).Select(s => s.Name).FirstOrDefault();
                    migrationResponse.DataFlowMigrationReason = "Success";
                }
                else
                {
                    string message = (sourceHasDataFlow) ? "Target dataflow metadata already exists" : "Source schema is not associated with dataflow";
                    migrationResponse.DataFlowMigrationReason = message;
                    migrationResponse.DataFlowName = (!sourceHasDataFlow) ? null : _datasetContext.DataFlow.Where(w => w.Id == newDataFlowId).Select(s => s.Name).FirstOrDefault();
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

                (string targetDatasetNamedEnvironment, NamedEnvironmentType targetDatasetNamedEnvironmentType) = _datasetContext.Datasets.Where(w => w.DatasetId == targetDatasetId).Select(s => new Tuple<string, NamedEnvironmentType>(s.NamedEnvironment, s.NamedEnvironmentType)).FirstOrDefault();

                //Adjust dataFlowDto associations and create new entity
                dataflowDto.Id = 0;
                dataflowDto.DatasetId = targetDatasetId;
                dataflowDto.NamedEnvironment = targetDatasetNamedEnvironment;
                dataflowDto.NamedEnvironmentType = targetDatasetNamedEnvironmentType;
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
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(MigrateDataset).ToLower()}";

            try
            {
                SchemaMigrationRequestResponse response = MigrateSchemaWithoutSave_Internal(migrationRequest);
                _datasetContext.SaveChanges();

                try
                {
                    if (response.MigratedSchema)
                    {
                        CreateExternalDependenciesForSchemas(new List<int> { response.TargetSchemaId });
                    }

                    if (response.MigratedDataFlow)
                    {
                        CreateExternalDependenciesForDataFlowBySchemaId(new List<int>() { response.TargetSchemaId });
                    }

                    if (response.MigratedSchemaRevision)
                    {
                        CreateExternalDependenciesForSchemaRevision(new List<(int, int)> { (response.TargetSchemaId, response.TargetSchemaRevisionId) });
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"{methodName} - Failed creating external dependencies", ex);

                    // Rollback newly created objects
                    Logger.Info($"{methodName} - Rollback initiated");
                    RollbackSchemaMigration(response);
                    _datasetContext.SaveChanges();
                    Logger.Info($"{methodName} - Rollback completed");
                    throw;
                }

                return response;
            }
            catch (Exception ex)
            {
                Logger.Error($"{methodName} Failed to perform migration", ex);
                _datasetContext.Clear();
                throw;
            }
        }

        internal virtual void CreateExternalDependenciesForDataset(List<int> datasetIdList)
        {
            foreach (int datasetId in datasetIdList)
            {
                DatasetService.CreateExternalDependencies(datasetId);
            }
        }

        internal virtual void CreateExternalDependenciesForSchemas(List<int> schemaIdList)
        {
            List<Task> tasks = new List<Task>();

            foreach (int schemaId in schemaIdList)
            {
                tasks.Add(SchemaService.CreateExternalDependenciesAsync(schemaId));
            }

            Task.WaitAll(tasks.ToArray());
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
                int dataFlowId = _datasetContext.DataFlow.First(w => w.SchemaId == schemaId && w.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active).Id;
                DataFlowService.CreateExternalDependencies(dataFlowId);
            }
        }

        internal virtual void RollbackDatasetMigration(DatasetMigrationRequestResponse response)
        {
            if (response.IsDatasetMigrated)
            {   
                //Delete dataset
                //  No need for any other deletes as this will remove all associated objects
                DatasetService.Delete(response.DatasetId, null, true);
            }
            else
            {
                //Issue you any schema migration rollbacks if necessary
                if (response.SchemaMigrationResponses.Count > 0)
                {
                    foreach(SchemaMigrationRequestResponse schemaResponse in response.SchemaMigrationResponses)
                    {
                        RollbackSchemaMigration(schemaResponse);
                    }
                }
            }
        }

        internal virtual void RollbackSchemaMigration(SchemaMigrationRequestResponse response)
        {
            if (response.MigratedSchema)
            {
                // Delete schema
                // No need for other deletes as this will take care of all associated objects
                int datasetFileConfigId = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == response.TargetSchemaId).Select(s => s.ConfigId).FirstOrDefault();
                ConfigService.Delete(datasetFileConfigId, null, true);
            }
            else 
            {
                if (response.MigratedSchemaRevision)
                {
                    RollbackSchemaRevisionMigration(response);
                }

                if (response.MigratedDataFlow)
                {
                    //Delete dataflow
                    DataFlowService.Delete(response.TargetDataFlowId, null, true);
                }
            }            
        }

        private void RollbackSchemaRevisionMigration(SchemaMigrationRequestResponse response)
        {
            //Delete schema revision
            List<BaseField> fieldList = _datasetContext.BaseFields.Where(w => w.ParentSchemaRevision.SchemaRevision_Id == response.TargetSchemaRevisionId).ToList();
            foreach (BaseField field in fieldList)
            {
                _datasetContext.RemoveById<BaseField>(field.FieldId);
            }

            _datasetContext.RemoveById<SchemaRevision>(response.TargetSchemaRevisionId);
        }

        /// <summary>
        /// Validates schema migration request
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="AggregateException">Aggregation of all validation errors</exception>
        private List<string> ValidateMigrationRequest(SchemaMigrationRequest request, bool sourceSchemaHasDataflow)
        {
            if (request == null)
            {
                throw new ArgumentNullException($"{nameof(request)}");
            }

            List<string> errors = new List<string>();
            
            if (request.SourceSchemaId == 0)
            {
                errors.Add("SourceSchemaId is required");
            }

            if (request.SourceSchemaId < 0)
            {
                errors.Add("SourceSchemaId cannot be a negative number");
            }            

            if (request.SourceSchemaId > 0 && request.TargetDatasetId > 0)
            {

                int sourceDatasetId = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == request.SourceSchemaId).Select(w => w.ParentDataset.DatasetId).FirstOrDefault();

                if (sourceSchemaHasDataflow && string.IsNullOrWhiteSpace(request.TargetDataFlowNamedEnvironment))
                {
                    errors.Add("TargetDataFlowNamedEnvironment is required");
                }

                if (!AreDatasetsRelated(sourceDatasetId, request.TargetDatasetId))
                {
                    errors.Add("Source and target datasets are not related");
                }
            }

            ValidateMigrationRequest((BaseMigrationRequest)request).ForEach(i => errors.Add(i));

            return errors;            
        }

        internal Task<List<string>> ValidateMigrationRequest(DatasetMigrationRequest request)
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

            if (request.SourceDatasetId < 0)
            {
                errors.Add("SourceDatasetId cannot be a negative number");
            }

            if (request.TargetDatasetId > 0 && !AreDatasetsRelated(request.SourceDatasetId, request.TargetDatasetId))
            {
                errors.Add("Source and target datasets are not related");                
            }

            ValidateMigrationRequest((BaseMigrationRequest)request).ForEach(i => errors.Add(i));

            //Do not proceed on as next validation requires values, so return with error list
            if (errors.Any())
            {
                return Task.FromResult(errors);
            }

            // This "wrapping" of the async portion of this method is required so that the error checking above runs immediately.
            // See https://sonarqube.sentry.com/coding_rules?open=csharpsquid%3AS4457&rule_key=csharpsquid%3AS4457
            return ValidateMigrationRequestAsync();            
            async Task<List<string>> ValidateMigrationRequestAsync()
            {
                string saidKeyCode = _datasetContext.GetById<Dataset>(request.SourceDatasetId).Asset.SaidKeyCode;
                if (request.TargetDatasetId == 0 && !await IsNamedEnvironmentRelatedToSaidAsset(saidKeyCode, request.TargetDatasetNamedEnvironment, request.TargetDatasetNamedEnvironmentType))
                {
                    errors.Add("Target named environment is not related to SAID asset associated with target dataset");
                }

                return errors;
            }            
        }

        private List<string> ValidateMigrationRequest(BaseMigrationRequest request)
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

            if (request.TargetDatasetId == 0)
            {
                if (String.IsNullOrWhiteSpace(request.TargetDatasetNamedEnvironment))
                {
                    errors.Add("TargetDatasetNamedEnvironment is required");
                }
                else
                {
                    //Named Environment naming conventions from https://confluence.sentry.com/x/eQNvAQ
                    Regex namedEnvironmentRegex = new Regex("^[A-Z0-9]{1,10}$");
                    Match matchedNamedEnvironment = namedEnvironmentRegex.Match(request.TargetDatasetNamedEnvironment);
                    if (!matchedNamedEnvironment.Success)
                    {
                        errors.Add("Named environment must be alphanumeric, all caps, and less than 10 characters");
                    }
                }                
            }

            return errors;
        }

        internal async Task<bool> IsNamedEnvironmentRelatedToSaidAsset(string saidKeyCode, string targetNamedEnvironment, NamedEnvironmentType targetNamedEnvironmentType)
        {
            bool IsRelated = true;

            var results = await QuartermasterService.VerifyNamedEnvironmentAsync(saidKeyCode, targetNamedEnvironment, targetNamedEnvironmentType);

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
