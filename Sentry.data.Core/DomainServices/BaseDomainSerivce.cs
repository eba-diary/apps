using Microsoft.Extensions.Logging;
using Sentry.data.Core.DependencyInjection;

namespace Sentry.data.Core.DomainServices
{
    public class BaseDomainSerivce<T> where T : BaseDomainSerivce<T>
    {
        private readonly DomainServiceCommonDependency<T> _commonDependencies;
#pragma warning disable IDE1006 // Naming Styles
        protected ILogger _logger => _commonDependencies._logger;
        protected IDataFeatures _dataFeatures => _commonDependencies._dataFeatures;
#pragma warning restore IDE1006 // Naming Styles


        public BaseDomainSerivce(DomainServiceCommonDependency<T> commonDependency)
        {
            _commonDependencies = commonDependency;
        }
    }
}
