using AutoMapper;
using System.Reflection;

namespace Sentry.data.Web.API
{
    public static class AutoMapperHelper
    {
        public static IMapper InitMapper()
        {
            MapperConfiguration configuration = new MapperConfiguration(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
            return new Mapper(configuration);
        }
    }
}