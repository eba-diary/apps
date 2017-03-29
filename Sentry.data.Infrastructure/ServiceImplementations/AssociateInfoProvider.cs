using System;
using Sentry.Associates;

namespace Sentry.data.Infrastructure
{
    public class AssociateInfoProvider : IAssociateInfoProvider
    {
        private IAssociatesServiceClient _associateService;
        private Boolean _localCacheProcessing = false;
        private Object _lockObject = new Object();


        public AssociateInfoProvider()
        {
            _associateService = Sentry.Associates.AssociatesService.Create(Sentry.Configuration.Config.GetHostSetting("HrServiceUrl"));
            _associateService.UseCacheDb = true;
            //if you need to set credentials of the associate service (e.g. cross domain calls), do that here
            //_associateService.Credentials = new Net.NetworkCredential(
            //    Sentry.Configuration.Config.GetHostSetting("CrossDomainUserID"),
            //    Sentry.Configuration.Config.GetHostSetting("CrossDomainUserPassword"));

            PopulateCacheIfNeeded();
        }

        private void OnLoadCacheComplete(IAsyncResult result)
        {
            IAssociatesServiceClient service = (IAssociatesServiceClient)result.AsyncState;
            service.EndLoadLocalCache(result);
            _localCacheProcessing = false;
        }

        public Associate GetAssociateInfo(string associateId)
        {
            PopulateCacheIfNeeded();
            return _associateService.GetAssociateById(associateId, true);
        }

        private void PopulateCacheIfNeeded()
        {
            if (_associateService.HasLocalCache == false && _localCacheProcessing == false)
            {
                lock (_lockObject)
                {
                    if (_associateService.HasLocalCache == false && _localCacheProcessing == false)
                    {
                        _associateService.BeginLoadLocalCache(OnLoadCacheComplete, _associateService, true);
                        _localCacheProcessing = true;
                    }
                }
            }
        }
    }
}
