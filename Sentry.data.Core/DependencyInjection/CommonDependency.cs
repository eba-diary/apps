using Microsoft.Extensions.Logging;

namespace Sentry.data.Core.DependencyInjection
{
    public class CommonDependency<T>
    {
        public ILogger<T> _logger;
        public IDataFeatures _dataFeatures;

        public CommonDependency(ILogger<T> logger, IDataFeatures dataFeatures)
        {
            _logger = logger;
            _dataFeatures = dataFeatures;
        }
        
        public CommonDependency() { }
    }
}
