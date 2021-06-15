using Sentry.data.Core;
using Sentry.data.Web.Models.ApiModels.Config;
using Sentry.data.Web.Models.ApiModels.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public static class ConfigExtension
    {
        public static Core.FileSchemaDto ToDto(this EditSchemaModel model, Core.FileSchemaDto dto)
        {
            dto.Name = model.Name;
            dto.Description = model.Description;
            return dto;
        }

        public static Core.FileSchemaDto DatasetModelToDto(this DatasetModel model)
        {
            return new Core.FileSchemaDto()
            {
                Name = model.ConfigFileName,
                Description = model.ConfigFileDesc,
                Delimiter = model.Delimiter,
                HasHeader = model.HasHeader,
                FileExtensionId = model.FileExtensionId,
                ObjectStatus = model.ObjectStatus
            };
        }

        public static Core.DataSourceDto ToDto(this CreateSourceModel model)
        {
            return new Core.DataSourceDto()
            {
                OriginatingId = model.Id,
                RetrunUrl = model.ReturnUrl,
                Name = model.Name,
                SourceType = model.SourceType,
                AuthID = model.AuthID,
                IsUserPassRequired = model.IsUserPassRequired,
                PortNumber = model.PortNumber,
                BaseUri = model.BaseUri,
                TokenAuthHeader = model.TokenAuthHeader,
                TokenAuthValue = model.TokenAuthValue,
                ClientId = model.ClientId,
                ClientPrivateId = model.ClientPrivateId,
                TokenUrl = model.TokenUrl,
                TokenExp = model.TokenExp,
                Scope = model.Scope,
                RequestHeaders = model.Headers
            };
        }

        public static Core.DataSourceDto ToDto(this DataSourceModel model)
        {
            return new Core.DataSourceDto()
            {
                OriginatingId = model.Id,
                RetrunUrl = model.ReturnUrl,
                Name = model.Name,
                Description = model.Description,
                SourceType = model.SourceType,
                AuthID = model.AuthID,
                IsUserPassRequired = model.IsUserPassRequired,
                PortNumber = model.PortNumber,
                BaseUri = model.BaseUri,
                TokenAuthHeader = model.TokenAuthHeader,
                TokenAuthValue = model.TokenAuthValue,
                ClientId = model.ClientId,
                ClientPrivateId = model.ClientPrivateId,
                TokenUrl = model.TokenUrl,
                TokenExp = model.TokenExp,
                Scope = model.Scope,
                RequestHeaders = model.Headers,
                IsSecured = model.IsSecured,
                PrimaryOwnerId = model.PrimaryOwnerId,
                PrimaryOwnerName = model.PrimaryOwnerName,
                PrimaryContactId= model.PrimaryContactId,
                PrimaryContactName = model.PrimaryContactName
            };
        }

        public static Core.DatasetFileConfigDto ToDto(this DatasetFileConfigsModel model)
        {
            List<Core.DataElementDto> deList = new List<Core.DataElementDto>();
            deList.Add(model.ToSchemaApiDto());

            //List<Core.SchemaDto> schemaList = new List<Core.SchemaDto>();
            //schemaList.Add(model.ToSchemaDto());

            return new Core.DatasetFileConfigDto()
            {
                ConfigId = model.ConfigId,
                Name = model.ConfigFileName,
                Description = model.ConfigFileDesc,
                DatasetScopeTypeId = model.DatasetScopeTypeID,
                ParentDatasetId = model.DatasetId,
                FileTypeId = model.FileTypeId,
                StorageCode = model.RawStorageId,
                Schemas = deList,
                FileExtensionId = model.FileExtensionID,
                IsInSAS = model.IncludedInSAS,
                HasHeader = model.HasHeader,
                Delimiter = model.Delimiter,
                CreateCurrentView = model.CreateCurrentView,
                ObjectStatus = model.ObjectStatus,
                SchemaRootPath = model.SchemaRootPath
            };
        }

        public static Core.FileSchemaDto ToSchema(this DatasetFileConfigsModel model)
        {
            return new Core.FileSchemaDto()
            {
                Name = model.ConfigFileName,
                SchemaEntity_NME = "",
                FileExtensionId = model.FileExtensionID,
                Delimiter = model.Delimiter,
                HasHeader = model.HasHeader,
                CreateCurrentView = model.CreateCurrentView,
                IsInSas = model.IncludedInSAS,
                SasLibrary = model.SasLibrary,
                ParentDatasetId = model.DatasetId,
                SchemaId = model.SchemaId,
                Description = model.ConfigFileDesc,
                CLA1396_NewEtlColumns = model.CLA1396_NewEtlColumns,
                CLA1580_StructureHive = model.CLA1580_StructureHive,
                CLA2472_EMRSend = model.CLA2472_EMRSend,
                CLA1286_KafkaFlag = model.CLA1286_KafkaFlag,
                CLA3014_LoadDataToSnowflake = model.CLA3014_LoadDataToSnowflake,
                ObjectStatus = model.ObjectStatus,
                SchemaRootPath = model.SchemaRootPath
            };
        }

        public static Core.DataElementDto ToSchemaApiDto(this DatasetFileConfigsModel model)
        {
            return new Core.DataElementDto()
            {
                DataElementID = model.DataElement_ID,
                DataElementName = model.ConfigFileName,
                SchemaName = model.ConfigFileName,
                SchemaDescription = model.ConfigFileDesc,
                Delimiter = model.Delimiter,
                HasHeader = model.HasHeader,
                CreateCurrentView = model.CreateCurrentView,
                FileFormatId = model.FileTypeId,
                ParentDatasetId = model.DatasetId,
                FileExtensionId = model.FileExtensionID,
                IsInSAS = model.IncludedInSAS,
                SasLibrary = model.SasLibrary
            };
        }

        public static ConfigInfoModel ToModel(this Core.DatasetFileConfigDto dto)
        {
            return new ConfigInfoModel()
            {
                ConfigId = dto.ConfigId,
                Name = dto.Name,
                Description = dto.Description,
                StorageCode = dto.StorageCode,
                FileType = dto.FileTypeId.ToString(),
                Delimiter = dto.Delimiter,
                HasHeader = dto.HasHeader,
                IsInSAS = dto.IsInSAS,
                CreateCurrentView = dto.CreateCurrentView,
                IsTrackableSchema = dto.IsTrackableSchema,
                SchemaId = (dto.Schema != null) ? dto.Schema.SchemaId : -1
            };
        }

        public static List<ConfigInfoModel> ToModel(this List<Core.DatasetFileConfigDto> dtoList)
        {
            List<ConfigInfoModel> modelList = new List<ConfigInfoModel>();
            foreach(Core.DatasetFileConfigDto dto in dtoList)
            {
                modelList.Add(dto.ToModel());
            }
            return modelList;
        }

        public static SchemaInfoModel ToSchemaModel(this Core.DatasetFileConfigDto dto)
        {
            Core.FileSchemaDto schemaDto = dto.Schema;
            return new SchemaInfoModel()
            {
                ConfigId = dto.ConfigId,
                Name = schemaDto.Name,
                SchemaId = schemaDto.SchemaId,
                SchemaEntity_NME = schemaDto.SchemaEntity_NME,
                Description = schemaDto.Description,
                StorageCode = schemaDto.StorageCode,
                Format = schemaDto.FileExtenstionName,
                CurrentView = schemaDto.CreateCurrentView,
                IsInSAS = schemaDto.IsInSas,
                Delimiter = schemaDto.Delimiter,
                HasHeader = schemaDto.HasHeader,
                IsTrackableSchema = dto.IsTrackableSchema,
                HiveTable = schemaDto.HiveTable,
                HiveDatabase = schemaDto.HiveDatabase,
                HiveTableStatus = schemaDto.HiveStatus,
                HiveLocation = schemaDto.HiveLocation,
                Options = new List<string>()
                {
                    "CLA1396_NewEtlColumns|" + schemaDto.CLA1396_NewEtlColumns.ToString(),
                    "CLA1580_StructureHive|" + schemaDto.CLA1580_StructureHive.ToString(),
                    "CLA2472_EMRSend|" + schemaDto.CLA2472_EMRSend.ToString(),
                    "CLA1286_KafkaFlag|" + schemaDto.CLA1286_KafkaFlag.ToString(),
                    "CLA3014_LoadDataToSnowflake|" + schemaDto.CLA3014_LoadDataToSnowflake.ToString()
                },
                DeleteInd = schemaDto.DeleteInd,
                SnowflakeDatabase = schemaDto.SnowflakeDatabase,
                SnowflakeSchema = schemaDto.SnowflakeSchema,
                SnowflakeTable = schemaDto.SnowflakeTable,
                SnowflakeStatus = schemaDto.SnowflakeStatus,
                ObjectStatus = schemaDto.ObjectStatus.GetDescription().ToUpper(),
                SchemaRootPath = schemaDto.SchemaRootPath?.Split(',')
            };
        }

        public static List<SchemaInfoModel> ToSchemaModel(this List<Core.DatasetFileConfigDto> dtoList)
        {
            List<SchemaInfoModel> modelList = new List<SchemaInfoModel>();
            foreach (Core.DatasetFileConfigDto dto in dtoList)
            {
                modelList.Add(dto.ToSchemaModel());
            }
            return modelList;
        }

        public static void ToModel(this DatasetFileConfigsModel model, Core.DatasetFileConfig config)
        {
            model.ConfigId = config.ConfigId;
            model.FileTypeId = config.FileTypeId;
            model.ConfigFileName = config.Schema.Name;
            model.ConfigFileDesc = config.Schema.Description;
            model.ParentDatasetName = config.ParentDataset.DatasetName;
            model.DatasetScopeTypeID = config.DatasetScopeType.ScopeTypeId;
            model.ScopeType = config.DatasetScopeType;
            model.FileExtensionID = config.Schema.Extension.Id;
            model.FileExtension = config.Schema.Extension;
            model.Schemas = config.Schemas;
            model.Schema = config.Schema ?? null;
            model.RawStorageId = config.Schema.StorageCode;
            model.Delimiter = config.Schema?.Delimiter;
            model.SchemaId = (config.Schema != null) ? config.Schema.SchemaId : 0;
            model.CreateCurrentView = (config.Schema != null) ? config.Schema.CreateCurrentView : false;
            model.HasHeader = (config.Schema != null) ? config.Schema.HasHeader : false;
            model.OldSchemaId = (model.Schemas.Any()) ? model.Schemas.FirstOrDefault().DataElement_ID : 0;
            model.CLA1396_NewEtlColumns = (config.Schema != null) ? config.Schema.CLA1396_NewEtlColumns : false;
            model.CLA1580_StructureHive = (config.Schema != null) ? config.Schema.CLA1580_StructureHive : false;
            model.CLA2472_EMRSend = (config.Schema != null) ? config.Schema.CLA2472_EMRSend : false;
            model.CLA1286_KafkaFlag = (config.Schema != null) ? config.Schema.CLA1286_KafkaFlag : false;
            model.CLA3014_LoadDataToSnowflake = (config.Schema != null) ? config.Schema.CLA3014_LoadDataToSnowflake : false;
            model.DeleteInd = config.Schema.DeleteInd;
            model.ObjectStatus = config.ObjectStatus;
            model.SchemaRootPath = config.Schema?.SchemaRootPath;
        }
    }
}