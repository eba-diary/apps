using AutoMapper;
using Sentry.data.Core;
using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class GlobalDatasetProfile : Profile
    {
        public GlobalDatasetProfile()
        {
            CreateMap<FilterCategoryOptionRequestModel, FilterCategoryOptionDto>(MemberList.Source)
                .ForMember(dest => dest.Selected, x => x.MapFrom(src => true));

            CreateMap<FilterCategoryRequestModel, FilterCategoryDto>(MemberList.Source)
                .ForMember(dest => dest.CategoryOptions, x => x.MapFrom<FilterCategoryOptionDenormalizeResolver>());

            CreateMap<SearchGlobalDatasetsRequestModel, BaseFilterSearchDto>(MemberList.Source).IncludeAllDerived();
            CreateMap<SearchGlobalDatasetsRequestModel, SearchGlobalDatasetsDto>(MemberList.Source);

            CreateMap<SearchGlobalDatasetResultDto, SearchGlobalDatasetResponseModel>();
            CreateMap<SearchGlobalDatasetsResultDto, SearchGlobalDatasetsResponseModel>();
        }
    }

    public class FilterCategoryOptionDenormalizeResolver : IValueResolver<FilterCategoryRequestModel, FilterCategoryDto, List<FilterCategoryOptionDto>>
    {
        public List<FilterCategoryOptionDto> Resolve(FilterCategoryRequestModel source, FilterCategoryDto destination, List<FilterCategoryOptionDto> destMember, ResolutionContext context)
        {
            foreach (FilterCategoryOptionRequestModel optionModel in source.CategoryOptions)
            {
                optionModel.OptionValue = FilterCategoryOptionNormalizer.Denormalize(source.CategoryName, optionModel.OptionValue);
                destMember.Add(context.Mapper.Map<FilterCategoryOptionDto>(optionModel));
            }

            return destMember;
        }
    }
}