using Microsoft.Extensions.Logging;

namespace Sentry.data.Core.DependencyInjection
{
    public class DomainServiceCommonDependency<T> : CommonDependency<T>
    {
        public DomainServiceCommonDependency(ILogger<T> logger, IDataFeatures dataFeatures) : base(logger, dataFeatures)
        {
        }

        public DomainServiceCommonDependency() { }
    }
}
