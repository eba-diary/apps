using Microsoft.Extensions.Logging;

namespace Sentry.data.Core.DependencyInjection
{
    public class CommonDependency<T>
    {
        protected ILogger<T> _logger;
        protected IDataFeatures _dataFeatures;

        public CommonDependency(ILogger<T> logger, IDataFeatures dataFeatures)
        {
            _logger = logger;
            _dataFeatures = dataFeatures;
        }

        public ILogger<T> Logger { get { return _logger; } }
        public IDataFeatures DataFeatures { get { return _dataFeatures; } }
        
        public CommonDependency() { }
    }
}
