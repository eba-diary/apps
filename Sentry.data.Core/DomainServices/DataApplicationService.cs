using Hangfire;
using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.Migration;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.DomainServices
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

        public DataApplicationService(IDatasetContext datasetContext,
            Lazy<IDatasetService> datasetService, Lazy<IConfigService> configService,
            Lazy<IDataFlowService> dataFlowService, Lazy<IBackgroundJobClient> hangfireBackgroundJobClient,
            Lazy<IUserService> userService, Lazy<IDataFeatures> dataFeatures,
            Lazy<ISecurityService> securityService, Lazy<ISchemaService> schemaService)
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


        public bool MigrateDataset(DatasetMigrationRequest migrationRequest)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(MigrateDataset).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            Logger.Info($"{methodName} {JsonConvert.SerializeObject(migrationRequest)}");
            try
            {
                IApplicationUser user = UserService.GetCurrentUser();
                UserSecurity sc = SecurityService.GetUserSecurity(_datasetContext.GetById<Dataset>(migrationRequest.SourceDatasetId), user);
                //if (!(sc.CanManageSchema || sc.CanCreateDataset))
                if (!sc.CanEditDataset)
                {
                    throw new DatasetUnauthorizedAccessException();
                }
                
                //TODO: CLA-4780 Add migration request validation

                DatasetDto dto = DatasetService.GetDatasetDto(migrationRequest.SourceDatasetId);

                dto.DatasetId = 0;
                dto.NamedEnvironment = migrationRequest.TargetDatasetNamedEnvironment;

                int newDsId = CreateWithoutSave(dto);

                if (newDsId == 0)
                {
                    throw new DataApplicationServiceException("Failed to create dataset metadata");
                }

                List<int> newSchemaIdList = MigrateSchemaWithoutSave_Internal(migrationRequest.SchemaMigrationRequests);

                //All entity objects have been created, therefore, save changes
                _datasetContext.SaveChanges();

                //Create dataset external dependencies
                CreateExternalDependenciesForDataset(new List<int>() { newDsId });

                //Create schema external dependencies
                CreateExternalDependenciesForSchema(newSchemaIdList);

                //Create dataflow external dependencies
                CreateExternalDependenciesForDataFlowBySchemaId(newSchemaIdList);

            }
            catch (Exception ex)
            {
                Logger.Error($"{methodName} Failed to perform migration", ex);
                _datasetContext.Clear();
                throw;
            }

            Logger.Info($"{methodName} Method End");
            return true;
        }

        public bool MigrateSchema(SchemaMigrationRequest request)
        {
            return MigrateSchema_Internal(request);
        }
        #endregion

        #region Private methods        
        /// <summary>
        /// Creates new dataset entity object.  
        /// <para>When saveChanges true, method will save changes to domain context and on failure clear domain context.</para>
        /// <para>When saveChanges false, caller is responsible for saving to context and 
        /// calling <see cref="SecurityService.EnqueueCreateDefaultSecurityForDataset(int)"></see> to create security groups.</para>
        /// </summary>
        /// <param name="dto">DatasetDto</param>
        /// <returns>Id of new dataset object.</returns>
        internal int Create(DatasetDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(Create).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            int newObjectId;
            try
            {
                newObjectId = CreateWithoutSave(dto);
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
                CreateExternalDependenciesForDataset(new List<int>() { newObjectId });
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to create external dependencies for dataflow", ex);
            }

            Logger.Info($"{methodName} Method End");
            return newObjectId;
        }
        /// <summary>
        /// Creates new datasetfileconfig entity object.  
        /// <para>When saveChanges true, method will save changes to domain context and on failure clear domain context.</para> 
        /// <para>If saveChanges false, calling method is in charge of save logic.</para>
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="saveChanges"></param>
        /// <returns>Id of new datasetfileconfig object.</returns>
        internal int Create(DatasetFileConfigDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(Create).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            int newObjectId;
            try
            {
                newObjectId = CreateWithoutSave(dto);
                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to save DatasetFileConfig", ex);
                _datasetContext.Clear();
                throw;
            }

            Logger.Info($"{methodName} Method End");
            return newObjectId;
        }
        /// <summary>
        /// Creates new dataflow entity object.  
        /// <para>When saveChanges true, method will save changes to domain context and on failure clear domain context.</para> 
        /// <para>When saveChanges false, caller is responsible for saving to context and 
        /// calling <see cref="SecurityService.EnqueueCreateDefaultSecurityForDataFlow(int)"></see> to create security groups.</para>
        /// </summary>
        /// <param name="dto">DataFlowDtoDto</param>
        /// <param name="saveChanges"></param>
        /// <returns>Id of new fileschema object.</returns>
        internal int Create(DataFlowDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(Create).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            int newObjectId;
            try
            {
                newObjectId = CreateWithoutSave(dto);
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
                CreateExternalDependenciesForDataFlow(new List<int>() { newObjectId });
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to create external dependencies for dataflow", ex);
            }


            Logger.Info($"{methodName} Method End");
            return newObjectId;
        }
        /// <summary>
        /// Creates new fileschema entity object.  
        /// <para>When saveChanges true, method will save changes to domain context and on failure clear domain context.</para> 
        /// <para>If saveChanges false, calling method is in charge of save logic.</para>
        /// </summary>
        /// <param name="dto">FileSchemaDto</param>
        /// <param name="saveChanges"></param>
        /// <returns>Id of new fileschema object.</returns>
        internal int Create(FileSchemaDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(Create).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            int newObjectId;
            try
            {
                newObjectId = CreateWithoutSave(dto);
                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to save FileSchema", ex);
                _datasetContext.Clear();
                throw;
            }

            try
            {
                CreateExternalDependenciesForSchema(new List<int>() { newObjectId });
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to create external dependencies for fileschema", ex);
            }

            Logger.Info($"{methodName} Method End");
            return newObjectId;
        }


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
        internal virtual int CreateWithoutSave(DatasetFileConfigDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(CreateWithoutSave).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            int configId;
            try
            {
                configId = ConfigService.Create(dto);
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to create DatasetFileConfig", ex);
                throw;
            }

            Logger.Info($"{methodName} Method End");
            return configId;
        }
        internal virtual int CreateWithoutSave(DataFlowDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(CreateWithoutSave).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            DataFlow newDataFlow;
            try
            {
                //Create dataflow / dataflowstep entities
                newDataFlow = DataFlowService.Create(dto);

                //Create retriever job entities
                DataFlowService.CreateDataFlowRetrieverJobMetadata(newDataFlow);
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to create DataFlow", ex);
                throw;
            }
            Logger.Info($"{methodName} Method End");
            return newDataFlow.Id;
        }
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


        internal virtual List<int> MigrateSchemaWithoutSave_Internal(List<SchemaMigrationRequest> requestList)
        {
            List<int> newSchemaIdList = new List<int>();
            foreach (SchemaMigrationRequest schemaMigration in requestList)
            {
                int newSchemaId = MigrateSchemaWithoutSave_Internal(schemaMigration);
                if (newSchemaId == 0)
                {
                    throw new DataApplicationServiceException($"Schema migration failed");
                }
                newSchemaIdList.Add(newSchemaId);
            }
            return newSchemaIdList;
        }
        internal int MigrateSchemaWithoutSave_Internal(SchemaMigrationRequest request)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(MigrateSchemaWithoutSave_Internal).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            Logger.Info($"{methodName} Processing : {JsonConvert.SerializeObject(request)}");

            int newSchemaId;
            try
            {
                //Retrieve source dto objects
                FileSchemaDto schemaDto = SchemaService.GetFileSchemaDto(request.SourceSchemaId);

                int sourceDatasetId = _datasetContext.DatasetFileConfigs.FirstOrDefault(w => w.Schema.SchemaId == request.SourceSchemaId).ParentDataset.DatasetId;
                DatasetFileConfigDto configDto = ConfigService.GetDatasetFileConfigDtoByDataset(sourceDatasetId).FirstOrDefault(x => x.Schema.SchemaId == request.SourceSchemaId);

                int dataflowId = _datasetContext.DataFlow.FirstOrDefault(w => w.SchemaId == request.SourceSchemaId).Id;
                DataFlowDetailDto dataflowDto = DataFlowService.GetDataFlowDetailDto(dataflowId);

                //Adjust schemaDto associations and create new entity
                schemaDto.ParentDatasetId = request.TargetDatasetId;
                newSchemaId = CreateWithoutSave(schemaDto);

                //Adjust datasetFileConfigDto associations and create new entity
                configDto.SchemaId = newSchemaId;
                configDto.ParentDatasetId = request.TargetDatasetId;
                _ = CreateWithoutSave(configDto);

                //Adjust dataFlowDto associations and create new entity
                dataflowDto.DatasetId = request.TargetDatasetId;
                dataflowDto.SchemaId = newSchemaId;
                dataflowDto.SchemaMap.First().SchemaId = newSchemaId;
                dataflowDto.SchemaMap.First().DatasetId = request.TargetDatasetId;
                dataflowDto.SchemaMap.First().StepId = 0;

                _ = CreateWithoutSave(dataflowDto);
            }
            catch (Exception ex)
            {
                Logger.Error($"{methodName} - Failed to migrate schema.", ex);
                throw;
            }

            Logger.Info($"{methodName} Method End");
            return newSchemaId;
        }
        internal bool MigrateSchema_Internal(SchemaMigrationRequest migrationRequest)
        {
            try
            {
                int newSchemaId = MigrateSchemaWithoutSave_Internal(migrationRequest);
                _datasetContext.SaveChanges();

                CreateExternalDependenciesForSchema(new List<int>() { newSchemaId });

                CreateExternalDependenciesForDataFlowBySchemaId(new List<int>() { newSchemaId });

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        internal virtual void CreateExternalDependenciesForDataset(List<int> datasetIdList)
        {
            foreach (int datasetId in datasetIdList)
            {
                DatasetService.CreateExternalDependencies(datasetId);
            }
        }
        internal virtual void CreateExternalDependenciesForSchema(List<int> schemaIdList)
        {
            foreach (int schemaId in schemaIdList)
            {
                SchemaService.CreateExternalDependencies(schemaId);
            }
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
