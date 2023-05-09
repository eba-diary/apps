using AutoMapper;
using Sentry.data.Core;
using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class GlobalDatasetProfile : Profile
    {
        public GlobalDatasetProfile()
        {
            CreateMap<BaseFilterCategoryOptionModel, FilterCategoryOptionDto>(MemberList.Source).IncludeAllDerived();
            CreateMap<FilterCategoryOptionRequestModel, FilterCategoryOptionDto>(MemberList.Source)
                .ForMember(dest => dest.Selected, x => x.MapFrom(src => true));

            CreateMap<BaseFilterCategoryModel, FilterCategoryDto>(MemberList.Source).IncludeAllDerived();
            CreateMap<FilterCategoryRequestModel, FilterCategoryDto>(MemberList.Source)
                .ForMember(dest => dest.CategoryOptions, x => x.MapFrom<FilterCategoryOptionDenormalizeResolver>());

            CreateMap<BaseGlobalDatasetRequestModel, BaseFilterSearchDto>().IncludeAllDerived();

            CreateMap<SearchGlobalDatasetsRequestModel, SearchGlobalDatasetsDto>()
                .ForMember(dest => dest.UseHighlighting, x => x.MapFrom(src => true));

            CreateMap<SearchHighlightDto, SearchHighlightModel>().ReverseMap();

            CreateMap<SearchGlobalDatasetDto, SearchGlobalDatasetResponseModel>();
            CreateMap<SearchGlobalDatasetsResultsDto, SearchGlobalDatasetsResponseModel>();

            CreateMap<GetGlobalDatasetFiltersRequestModel, GetGlobalDatasetFiltersDto>();

            CreateMap<FilterCategoryOptionDto, BaseFilterCategoryOptionModel>(MemberList.Destination).IncludeAllDerived();
            CreateMap<FilterCategoryOptionDto, FilterCategoryOptionResponseModel>()
                .ForMember(dest => dest.OptionValue, x => x.MapFrom(src => FilterCategoryOptionNormalizer.Normalize(src.ParentCategoryName, src.OptionValue)));

            CreateMap<FilterCategoryDto, BaseFilterCategoryModel>(MemberList.Destination).IncludeAllDerived();
            CreateMap<FilterCategoryDto, FilterCategoryResponseModel>();

            CreateMap<GetGlobalDatasetFiltersResultDto, GetGlobalDatasetFiltersResponseModel>();
        }
    }

    public class FilterCategoryOptionDenormalizeResolver : IValueResolver<FilterCategoryRequestModel, FilterCategoryDto, List<FilterCategoryOptionDto>>
    {
        public List<FilterCategoryOptionDto> Resolve(FilterCategoryRequestModel source, FilterCategoryDto destination, List<FilterCategoryOptionDto> destMember, ResolutionContext context)
        {
            if (destMember == null)
            {
                destMember = new List<FilterCategoryOptionDto>();
            }

            foreach (BaseFilterCategoryOptionModel optionModel in source.CategoryOptions)
            {
                optionModel.OptionValue = FilterCategoryOptionNormalizer.Denormalize(source.CategoryName, optionModel.OptionValue);
                FilterCategoryOptionDto optionDto = context.Mapper.Map<FilterCategoryOptionDto>(optionModel);
                optionDto.ParentCategoryName = source.CategoryName;
                destMember.Add(optionDto);
            }

            return destMember;
        }
    }
}