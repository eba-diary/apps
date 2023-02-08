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

            CreateMap<SampleDto, SampleResponseViewModel>().ForMember(d => d.Id, x => x.MapFrom(s => s.SampleId)).IncludeAllDerived();
            CreateMap<SampleDto, AddSampleResponseViewModel>();
            CreateMap<SampleDto, UpdateSampleResponseViewModel>();
            CreateMap<SampleDto, GetSampleResponseViewModel>();
        }
    }
}