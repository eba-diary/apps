using AutoMapper;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;

namespace Sentry.data.Web.API
{
    public class SchemaProfile : Profile
    {
        public SchemaProfile()
        {
            //FileSchemaDto
            CreateMap<BaseSchemaModel, FileSchemaDto>(MemberList.None)
                .ForMember(dest => dest.Description, x => x.MapFrom(src => src.SchemaDescription))
                .ForMember(dest => dest.FileExtensionName, x =>
                {
                    x.PreCondition(src => !string.IsNullOrWhiteSpace(src.FileTypeCode));
                    x.MapFrom(src => src.FileTypeCode);
                    x.AddTransform(value => value.ToUpper());
                })
                .ForMember(dest => dest.CLA1286_KafkaFlag, x => x.MapFrom(src => src.IngestionTypeCode == IngestionType.Topic.ToString()))
                .IncludeAllDerived();

            CreateMap<BaseImmutableSchemaModel, FileSchemaDto>(MemberList.None)
                .ForMember(dest => dest.ParentDatasetId, x => x.MapFrom(src => src.DatasetId))
                .ForMember(dest => dest.Name, x => x.MapFrom(src => src.SchemaName))
                .IncludeAllDerived();

            CreateMap<AddSchemaRequestModel, FileSchemaDto>(MemberList.None)
                .ForMember(dest => dest.ObjectStatus, x => x.MapFrom(src => ObjectStatusEnum.Active));

            CreateMap<UpdateSchemaRequestModel, FileSchemaDto>(MemberList.None);

            //DatasetFileConfigDto
            CreateMap<BaseSchemaModel, DatasetFileConfigDto>(MemberList.None)
                .ForMember(dest => dest.Description, x => x.MapFrom(src => src.SchemaDescription))
                .ForMember(dest => dest.DatasetScopeTypeName, x => x.MapFrom(src => src.ScopeTypeCode))
                .ForMember(dest => dest.FileTypeId, x => x.MapFrom(src => (int)FileType.DataFile))
                .IncludeAllDerived();

            CreateMap<BaseImmutableSchemaModel, DatasetFileConfigDto>(MemberList.None)
                .ForMember(dest => dest.ParentDatasetId, x => x.MapFrom(src => src.DatasetId))
                .ForMember(dest => dest.Name, x => x.MapFrom(src => src.SchemaName)).IncludeAllDerived();

            CreateMap<AddSchemaRequestModel, DatasetFileConfigDto>(MemberList.None)
                .ForMember(dest => dest.ObjectStatus, x => x.MapFrom(src => ObjectStatusEnum.Active));

            CreateMap<UpdateSchemaRequestModel, DatasetFileConfigDto>(MemberList.None);

            //DataFlowDto
            CreateMap<BaseSchemaModel, DataFlowDto>(MemberList.None)
                .ForMember(dest => dest.IngestionType, x =>
                {
                    x.PreCondition(src => !string.IsNullOrWhiteSpace(src.IngestionTypeCode));
                    x.MapFrom(src => (int)Enum.Parse(typeof(IngestionType), src.IngestionTypeCode, true));
                })
                .ForMember(dest => dest.CompressionType, x =>
                {
                    x.PreCondition(src => !string.IsNullOrWhiteSpace(src.CompressionTypeCode));
                    x.MapFrom(src => (int)Enum.Parse(typeof(CompressionTypes), src.CompressionTypeCode, true));
                })
                .ForMember(dest => dest.CompressionJob, x =>
                {
                    x.PreCondition(src => !string.IsNullOrWhiteSpace(src.CompressionTypeCode));
                    x.MapFrom(src => new CompressionJobDto { CompressionType = (CompressionTypes)Enum.Parse(typeof(CompressionTypes), src.CompressionTypeCode, true) });
                })
                .ForMember(dest => dest.IsPreProcessingRequired, x => x.MapFrom(src => src.IsPreprocessingRequired))
                .ForMember(dest => dest.PreProcessingOption, x =>
                {
                    x.MapFrom(src => !string.IsNullOrWhiteSpace(src.PreprocessingTypeCode) ? (int)Enum.Parse(typeof(DataFlowPreProcessingTypes), src.PreprocessingTypeCode, true) : 0);
                })
                .ForMember(dest => dest.TopicName, x => x.MapFrom(src => src.KafkaTopicName))
                .IncludeAllDerived();

            CreateMap<BaseImmutableSchemaModel, DataFlowDto>(MemberList.None)
                .ForMember(dest => dest.SaidKeyCode, x =>
                {
                    x.PreCondition(src => !string.IsNullOrWhiteSpace(src.SaidAssetCode));
                    x.MapFrom(src => src.SaidAssetCode);
                    x.AddTransform(value => value.ToUpper());
                })
                .ForMember(dest => dest.NamedEnvironmentType, x =>
                {
                    x.PreCondition(src => !string.IsNullOrWhiteSpace(src.NamedEnvironmentTypeCode));
                    x.MapFrom(src => (NamedEnvironmentType)Enum.Parse(typeof(NamedEnvironmentType), src.NamedEnvironmentTypeCode, true));
                })
                .IncludeAllDerived();

            CreateMap<AddSchemaRequestModel, DataFlowDto>(MemberList.None)
                .ForMember(dest => dest.ObjectStatus, x => x.MapFrom(src => ObjectStatusEnum.Active));

            CreateMap<UpdateSchemaRequestModel, DataFlowDto>(MemberList.None);

            //AddSchemaDto
            CreateMap<AddSchemaRequestModel, SchemaFlowDto>(MemberList.None)
                .ForMember(dest => dest.SchemaDto, x => x.MapFrom(src => src))
                .ForMember(dest => dest.DatasetFileConfigDto, x => x.MapFrom(src => src))
                .ForMember(dest => dest.DataFlowDto, x => x.MapFrom(src => src));

            //UpdateSchemaDto
            CreateMap<UpdateSchemaRequestModel, SchemaFlowDto>(MemberList.None)
                .ForMember(dest => dest.SchemaDto, x => x.MapFrom(src => src))
                .ForMember(dest => dest.DatasetFileConfigDto, x => x.MapFrom(src => src))
                .ForMember(dest => dest.DataFlowDto, x => x.MapFrom(src => src));

            //BaseSchemaResponseModel
            CreateMap<SchemaResultDto, BaseSchemaModel>(MemberList.Destination)
                .ForMember(dest => dest.IngestionTypeCode, x => x.MapFrom(src => src.IngestionType.ToString()))
                .IncludeAllDerived();

            CreateMap<SchemaResultDto, BaseImmutableSchemaModel>(MemberList.Destination)
                .ForMember(dest => dest.NamedEnvironmentTypeCode, x => x.MapFrom(src => src.NamedEnvironmentType.ToString()))
                .IncludeAllDerived();

            CreateMap<SchemaResultDto, BaseSchemaResponseModel>(MemberList.Destination)
                .ForMember(dest => dest.ObjectStatusCode, x => x.MapFrom(src => src.ObjectStatus.ToString()))
                .IncludeAllDerived();

            CreateMap<SchemaResultDto, AddSchemaResponseModel>(MemberList.Destination);

            CreateMap<SchemaResultDto, UpdateSchemaResponseModel>(MemberList.Destination);
        }
    }
}