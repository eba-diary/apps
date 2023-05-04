using AutoMapper;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Helpers;

namespace Sentry.data.Web
{
    public class ViewModelProfile : Profile
    {
        public ViewModelProfile()
        {
            CreateMap<GlobalDatasetViewModel, SearchGlobalDatasetDto>().ReverseMap();
            CreateMap<GlobalDatasetPageRequestViewModel, GlobalDatasetPageRequestDto>();

            CreateMap<GlobalDatasetPageResultDto, GlobalDatasetResultsViewModel>(MemberList.Destination)
                .ForMember(dest => dest.PageSizeOptions, x => x.MapFrom(src => Utility.BuildTilePageSizeOptions(src.PageSize.ToString())))
                .ForMember(dest => dest.SortByOptions, x => x.MapFrom(src => Utility.BuildSelectListFromEnum<GlobalDatasetSortByOption>(src.SortBy)))
                .ForMember(dest => dest.PageItems, x => x.MapFrom(src => Utility.BuildPageItemList(src.TotalResults, src.PageSize, src.PageNumber)))
                .ForMember(dest => dest.LayoutOptions, x => x.MapFrom(src => Utility.BuildSelectListFromEnum<LayoutOption>(src.Layout)));
        }
    }
}