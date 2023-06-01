using AutoMapper;
using Microsoft.Extensions.Logging;
using Sentry.data.Core;
using Sentry.data.Core.DependencyInjection;

namespace Sentry.data.Web.API
{
    public class ApiCommonDependency<T> : CommonDependency<T>
    {
        public IMapper Mapper { get; }
        public IValidationRegistry ValidationRegistry { get; }

        public ApiCommonDependency(IMapper mapper, IValidationRegistry validationRegistry, ILogger<T> logger, IDataFeatures dataFeatures) : base(logger, dataFeatures)
        {
            Mapper = mapper;
            ValidationRegistry = validationRegistry;
        }
    }
}