using AutoMapper;
using Sentry.data.Core;

namespace Sentry.data.Web.API
{
    public class ApiDependency : IApiDependency
    {
        public IMapper Mapper { get; }
        public IValidationRegistry ValidationRegistry { get; }
        public IDataFeatures DataFeatures { get; }

        public ApiDependency(IMapper mapper, IValidationRegistry validationRegistry, IDataFeatures dataFeatures)
        {
            Mapper = mapper;
            ValidationRegistry = validationRegistry;
            DataFeatures = dataFeatures;
        }
    }
}