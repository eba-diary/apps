using AutoMapper;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Helpers;
using Sentry.data.Web.Models.ApiModels.Schema;
using System;
using System.Linq;

namespace Sentry.data.Web.Helpers
{
    public static class MapConfig
    {
        private static IMapper _mapper;

        public static IMapper Mapper
        {
            get
            {
                if (_mapper == null)
                {
                    MapperConfiguration cfg = new MapperConfiguration((c) =>
                    {
                        c.CreateMap<SchemaInfoModel, FileSchemaDto>(MemberList.None).ForMember(dto => dto.CLA1286_KafkaFlag, m => m.MapFrom(mdl => mdl.Options.Any(a => string.Equals(a, "CLA1286_KafkaFlag|true", StringComparison.OrdinalIgnoreCase))))
                                                                                    .ForMember(dto => dto.CLA1396_NewEtlColumns, m => m.MapFrom(mdl => mdl.Options.Any(a => string.Equals(a, "CLA1396_NewEtlColumns|true", StringComparison.OrdinalIgnoreCase))))
                                                                                    .ForMember(dto => dto.CLA1580_StructureHive, m => m.MapFrom(mdl => mdl.Options.Any(a => string.Equals(a, "CLA1580_StructureHive|true", StringComparison.OrdinalIgnoreCase))))
                                                                                    .ForMember(dto => dto.CLA2472_EMRSend, m => m.MapFrom(mdl => mdl.Options.Any(a => string.Equals(a, "CLA2472_EMRSend|true", StringComparison.OrdinalIgnoreCase))))
                                                                                    .ForMember(dto => dto.CLA3014_LoadDataToSnowflake, m => m.MapFrom(mdl => mdl.Options.Any(a => string.Equals(a, "CLA3014_LoadDataToSnowflake|true", StringComparison.OrdinalIgnoreCase))))
                                                                                    .ForMember(dto => dto.CreateCurrentView, m => m.MapFrom(mdl => mdl.CurrentView))
                                                                                    .ForMember(dto => dto.HiveStatus, m => m.MapFrom(mdl => mdl.HiveTableStatus))
                                                                                    .ForMember(dto => dto.ObjectStatus, m => m.MapFrom(mdl => EnumHelper.GetByDescription<ObjectStatusEnum>(mdl.ObjectStatus)))
                                                                                    .ForMember(dto => dto.SchemaRootPath, m => m.MapFrom(mdl => string.Join(",", mdl.SchemaRootPath)));
                    });

                    _mapper = cfg.CreateMapper();
                }

                return _mapper;
            }
        }
    }
}