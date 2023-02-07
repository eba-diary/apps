using AutoMapper;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class SampleProfile : Profile
    {
        public SampleProfile()
        {
            CreateMap<SampleViewModel, SampleDto>().IncludeAllDerived().ReverseMap();
            CreateMap<AddSampleViewModel, SampleDto>();
            CreateMap<UpdateSampleViewModel, SampleDto>();

            CreateMap<ValidationResultDto, ValidationResultViewModel>();
        }
    }
}