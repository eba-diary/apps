using AutoMapper;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;

namespace Sentry.data.Web.API
{
    public class DatasetProfile : Profile
    {
        public DatasetProfile()
        {
            CreateMap<BaseDatasetModel, DatasetDto>(MemberList.Source)
                .ForMember(dest => dest.DatasetDesc, x => x.MapFrom(src => src.DatasetDescription))
                .ForMember(dest => dest.DatasetInformation, x => x.MapFrom(src => src.UsageInformation))
                .ForMember(dest => dest.DataClassification, x => x.MapFrom(src => Enum.Parse(typeof(DataClassificationType), src.DataClassificationTypeCode, true)))
                .ForMember(dest => dest.OriginationId, x => x.MapFrom(src => (int)Enum.Parse(typeof(DatasetOriginationCode), src.OriginationCode, true)))
                .ForMember(dest => dest.CreationUserId, x => x.MapFrom(src => src.OriginalCreator))
                .ForMember(dest => dest.DatasetDtm, x => x.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.ChangedDtm, x => x.MapFrom(src => DateTime.Now))
                .IncludeAllDerived()
                .ReverseMap();

            CreateMap<DatasetModel, DatasetDto>(MemberList.Source)
                .ForMember(dest => dest.SAIDAssetKeyCode, x => x.MapFrom(src => src.SaidAssetCode))
                .ForMember(dest => dest.NamedEnvironmentType, x => x.MapFrom(src => Enum.Parse(typeof(NamedEnvironmentType), src.NamedEnvironmentTypeCode, true)))
                .IncludeAllDerived()
                .ReverseMap();

            CreateMap<AddDatasetRequestModel, DatasetDto>(MemberList.Source);

            CreateMap<UpdateDatasetRequestModel, DatasetDto>(MemberList.Source);
        }
    }
}