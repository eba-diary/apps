using Hangfire;
using Sentry.Common.Logging;
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
        #endregion

        #region Private methods        
        /// <summary>
        /// Creates new dataset entity object, adds to context, and saves changes.
        /// </summary>
        /// <param name="dto">DatasetDto</param>
        /// <returns></returns>
        internal int Create(DatasetDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(Create).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            int newDatasetId = 0;
            try
            {
                newDatasetId = DatasetService.Create(dto);

                _datasetContext.SaveChanges();

                if (DataFeatures.CLA3718_Authorization.GetValue())
                {
                    // Create a Hangfire job that will setup the default security groups for this new dataset
                    SecurityService.EnqueueCreateDefaultSecurityForDataset(newDatasetId);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to save Dataset", ex);
                _datasetContext.Clear();
                throw;
            }

            Logger.Info($"{methodName} Method End");
            return newDatasetId;
        }

        /// <summary>
        /// creates new datasetfileconfig entity object, adds to context, and saves changes
        /// </summary>
        /// <param name="dto">DatasetFileConfigDto</param>
        private void Create(DatasetFileConfigDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(Create).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            try
            {
                ConfigService.CreateAndSaveDatasetFileConfig(dto); 
                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Warn($"{methodName} - Failed to save DatasetFileConfig", ex);
                _datasetContext.Clear();
                throw;
            }
            Logger.Info($"{methodName} Method End");
        }

        /// <summary>
        /// Creates new fileschema entity object, adds to context, and saves changes
        /// </summary>
        /// <param name="dto">FileSchemaDto</param>
        /// <returns></returns>
        private int Create(FileSchemaDto dto)
        {
            string methodName = $"{nameof(DataApplicationService).ToLower()}_{nameof(Create).ToLower()}";
            Logger.Info($"{methodName} Method Start");
            int newFileSchemaId = 0;
            try
            {
                newFileSchemaId = SchemaService.Create(dto);
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
