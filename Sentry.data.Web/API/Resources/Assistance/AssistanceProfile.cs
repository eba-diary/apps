using AutoMapper;
using Sentry.data.Core;

namespace Sentry.data.Web.API
{
    public class AssistanceProfile : Profile
    {
        public AssistanceProfile() 
        {
            CreateMap<AddAssistanceRequestModel, AddAssistanceDto>();
            CreateMap<AddAssistanceResultDto, AddAssistanceResponseModel>();
        }
    }
}