using System;
using Sentry.Associates;
using System.Linq;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure
{
    public class AssociateInfoProvider : IAssociateInfoProvider
    {
        private readonly IAssociatesServiceClient _associateService;
        private bool _localCacheProcessing = false;
        private object _lockObject = new object();


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

        public Associate GetAssociateInfo(string associateId)
        {
            PopulateCacheIfNeeded();

            return _associateService.GetAssociateById(associateId, true);
        }

        public Associate GetAssociateInfoByName(string associateName)
        {
            PopulateCacheIfNeeded();

            return _associateService.GetAssociatesByName(associateName, true).First();
        }

        private void PopulateCacheIfNeeded()
        {
            if (_associateService.HasLocalCache == false && _localCacheProcessing == false)
            {
                lock (_lockObject)
                {
                    _localCacheProcessing = true;
                    _associateService.UseCacheDb = true;
                    _associateService.LoadLocalCacheAsync(true).ContinueWith(task =>
                       _localCacheProcessing = false
                    );
                }
            }
        }
    }
}
