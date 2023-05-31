using AutoMapper;
using Sentry.data.Core;

namespace Sentry.data.Web.API
{
    public interface IApiDependency
    {
        IMapper Mapper { get; }
        IValidationRegistry ValidationRegistry { get; }
        IDataFeatures DataFeatures { get; }
    }
}
