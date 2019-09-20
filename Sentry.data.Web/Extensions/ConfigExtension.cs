using System;
using System.Collections.Generic;
using Sentry.data.Web.Models.ApiModels.Config;
using Sentry.data.Web.Models.ApiModels.Schema;

namespace Sentry.data.Web
{
    public static class ConfigExtension
    {
        public static Core.DataElementDto ToDto(this EditSchemaModel model)
        {
            return new Core.DataElementDto()
            {
                SchemaName = model.Name,
                SchemaDescription = model.Description,
                SchemaIsForceMatch = model.IsForceMatch,
                SchemaIsPrimary = model.IsPrimary,
                Delimiter = model.Delimiter,
                DataElementChange_DTM = DateTime.Now,
                HasHeader = model.HasHeader,
                FileFormatId = model.FileTypeId
            };            
        }

        public static Core.DataElementDto DatasetModelToDto(this DatasetModel model)
        {
            return new Core.DataElementDto()
            {
                SchemaName = model.ConfigFileName,
                SchemaDescription = model.ConfigFileDesc,
                SchemaIsForceMatch = false,
                SchemaIsPrimary = true,
                Delimiter = model.Delimiter,
                DataElementChange_DTM = DateTime.Now,
                HasHeader = model.HasHeader,
                FileFormatId = model.FileExtensionId,
                IsInSAS = model.IncludeInSas
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
                CreateCurrentView = model.CreateCurrentView
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
            return new SchemaInfoModel()
            {
                ConfigId = dto.ConfigId,
                SchemaId = dto.Schema.SchemaId,
                Description = dto.Description,
                StorageCode = dto.StorageCode,
                FileType = dto.FileExtensionId.ToString(),
                CreateCurrentView = dto.CreateCurrentView,
                IsInSAS = dto.IsInSAS,
                Delimiter = dto.Delimiter,
                HasHeader = dto.HasHeader,
                IsTrackableSchema = dto.IsTrackableSchema
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
    }
}