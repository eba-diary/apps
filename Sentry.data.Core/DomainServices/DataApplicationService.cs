using Hangfire;
using Sentry.Common.Logging;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;

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

        public DataApplicationService(IDatasetContext datasetContext,
            Lazy<IDatasetService> datasetService,
            Lazy<IConfigService> configService,
            Lazy<IDataFlowService> dataFlowService,
            Lazy<IBackgroundJobClient> hangfireBackgroundJobClient,
            Lazy<IUserService> userService)
        {
            _datasetContext = datasetContext;
            _datasetService = datasetService;
            _configService = configService;
            _dataFlowService = dataFlowService;
            _hangfireBackgroundJobClient = hangfireBackgroundJobClient;
            _userService = userService;
        }

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
    }
}
