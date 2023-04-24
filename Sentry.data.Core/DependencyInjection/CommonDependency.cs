using Microsoft.Extensions.Logging;

namespace Sentry.data.Core.DependencyInjection
{
    public class CommonDependency
    {
        public ILogger _logger;
        public IDataFeatures _dataFeatures;

        public CommonDependency(ILogger logger, IDataFeatures dataFeatures)
        {
            _logger = logger;
            _dataFeatures = dataFeatures;
        }
        
        public CommonDependency() { }
    }
}
