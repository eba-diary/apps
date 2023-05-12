using Microsoft.Extensions.Logging;
using Sentry.data.Core.DependencyInjection;

namespace Sentry.data.Core.DomainServices
{
    public class BaseDomainService<T> where T : class
    {
        private readonly DomainServiceCommonDependency<T> _commonDependencies;
#pragma warning disable IDE1006 // Naming Styles
        protected ILogger _logger => _commonDependencies.Logger;
        protected IDataFeatures _dataFeatures => _commonDependencies.DataFeatures;
#pragma warning restore IDE1006 // Naming Styles


        public BaseDomainService(DomainServiceCommonDependency<T> commonDependency)
        {
            _commonDependencies = commonDependency;
        }
    }
}
