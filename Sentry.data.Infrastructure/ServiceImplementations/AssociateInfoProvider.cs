using System;
using Sentry.Associates;
using System.Linq;
using Sentry.data.Core;
using System.Threading.Tasks;

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

            var associateCacheOptions = new AssociatesCacheOptions
            {
                SuccessCallback = () =>
                {
                    Sentry.Common.Logging.Logger.Info("Associate Cache has been loaded");
                },
                ExceptionCallback = (ex, retryCount) =>
                {
                    Sentry.Common.Logging.Logger.Error($"There was an error loading the associate cache. Retry count: {retryCount}", ex);
                },
                IncludeInactive = true
            };
            _associateService.LoadLocalCacheWithRetry(associateCacheOptions);
        }

        public async Task<Associate> GetActiveAssociateByIdAsync(string associateId)
        {
            return await _associateService.GetAssociateByIdAsync(associateId);
        }

        public Associate GetAssociateInfo(string associateId)
        {
            return _associateService.GetAssociateById(associateId, true);
        }

        public Associate GetAssociateInfoByName(string associateName)
        {
            return _associateService.GetAssociatesByName(associateName, true).First();
        }
    }
}
