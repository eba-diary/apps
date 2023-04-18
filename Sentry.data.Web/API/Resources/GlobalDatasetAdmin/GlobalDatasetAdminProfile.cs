using AutoMapper;
using Sentry.data.Core;

namespace Sentry.data.Web.API
{
    public class GlobalDatasetAdminProfile : Profile
    {
        public GlobalDatasetAdminProfile()
        {
            CreateMap<IndexGlobalDatasetsRequestModel, IndexGlobalDatasetsDto>();
            CreateMap<IndexGlobalDatasetsResultDto, IndexGlobalDatasetsResponseModel>();
        }
    }
}