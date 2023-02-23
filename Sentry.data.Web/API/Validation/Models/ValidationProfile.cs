using AutoMapper;
using System.Linq;

namespace Sentry.data.Web.API.Validation
{
    public class ValidationProfile : Profile
    {
        public ValidationProfile()
        {
            CreateMap<ConcurrentFieldValidationResponse, FieldValidationResponseModel>()
                .ForMember(dest => dest.ValidationMessages, x => x.MapFrom(src => src.ValidationMessages.ToList()));

            CreateMap<ConcurrentValidationResponse, ValidationResponseModel>()
                .ForMember(dest => dest.FieldValidations, x => x.MapFrom(src => src.FieldValidations.ToList()));
        }
    }
}