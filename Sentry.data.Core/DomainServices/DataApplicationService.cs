using Sentry.Common.Logging;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.DomainServices
{
    public class DataApplicationService : IDataApplicationService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly Lazy<IDatasetService> _datasetService;
        private readonly Lazy<IConfigService> _configService;
        private readonly Lazy<IDataFlowService> _dataFlowService;

        public DataApplicationService(IDatasetContext datasetContext,
            Lazy<IDatasetService> datasetService,
            Lazy<IConfigService> configService,
            Lazy<IDataFlowService> dataFlowService)
        {
            _datasetContext = datasetContext;
            _datasetService = datasetService;
            _configService = configService;
            _dataFlowService = dataFlowService;
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


        public bool DeleteDataset(List<int> deleteIdList, IApplicationUser user, bool forceDelete = false)
        {
            return Delete(deleteIdList, user, DatasetService, forceDelete);
        }

        public bool DeleteDatasetFileConfig(List<int> deleteIdList, IApplicationUser user, bool forceDelete = false)
        {
            return Delete(deleteIdList, user, ConfigService, forceDelete);
        }

        public bool DeleteDataflow(List<int> deleteIdList, IApplicationUser user, bool forceDelete = false)
        {
            return Delete(deleteIdList, user, DataFlowService, forceDelete);
        }


        private bool Delete<T>(List<int> deleteIdList, IApplicationUser user, T instance, bool forceDelete = false) where T : IEntityService
        {
            string methodName = $"{nameof(DataApplicationService)}_{nameof(Delete)}";
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
