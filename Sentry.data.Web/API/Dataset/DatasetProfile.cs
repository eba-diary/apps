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
                .ForMember(dest => dest.DataClassification, x => x.MapFrom(src => (DataClassificationType)Enum.Parse(typeof(DataClassificationType), src.DataClassificationTypeCode, true)))
                .ForMember(dest => dest.OriginationId, x => x.MapFrom(src => (int)Enum.Parse(typeof(DatasetOriginationCode), src.OriginationCode, true)))
                .ForMember(dest => dest.CreationUserId, x => x.MapFrom(src => src.OriginalCreator))
                .ForMember(dest => dest.DatasetDtm, x => x.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.ChangedDtm, x => x.MapFrom(src => DateTime.Now))
                .IncludeAllDerived();

            CreateMap<ImmutableDatasetModel, DatasetDto>(MemberList.Source)
                .ForMember(dest => dest.SAIDAssetKeyCode, x => x.MapFrom(src => src.SaidAssetCode.ToUpper()))
                .ForMember(dest => dest.NamedEnvironmentType, x => x.MapFrom(src => (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType), src.NamedEnvironmentTypeCode, true)))
                .IncludeAllDerived();

            CreateMap<AddDatasetRequestModel, DatasetDto>(MemberList.Source);

            CreateMap<UpdateDatasetRequestModel, DatasetDto>(MemberList.Source);

            CreateMap<DatasetResultDto, BaseDatasetModel>(MemberList.Destination)
                .ForMember(dest => dest.DataClassificationTypeCode, x => x.MapFrom(src => src.DataClassificationType.ToString()))
                .ForMember(dest => dest.OriginationCode, x => x.MapFrom(src => src.OriginationCode.ToString()))
                .IncludeAllDerived();

            CreateMap<DatasetResultDto, ImmutableDatasetModel>(MemberList.Destination)
                .ForMember(dest => dest.NamedEnvironmentTypeCode, x => x.MapFrom(src => src.NamedEnvironmentType.ToString()))
                .IncludeAllDerived();

            CreateMap<DatasetResultDto, DatasetResponseModel>(MemberList.Destination)
                .ForMember(dest => dest.ObjectStatusCode, x => x.MapFrom(src => src.ObjectStatus.ToString()))
                .IncludeAllDerived();

            CreateMap<DatasetResultDto, AddDatasetResponseModel>(MemberList.Destination);

            CreateMap<DatasetResultDto, UpdateDatasetResponseModel>(MemberList.Destination);
        }
    }
}