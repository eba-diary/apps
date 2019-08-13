using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
                FileFormatId = model.FileExtensionId
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
            deList.Add(model.ToSchemaDto());

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
                FileExtensionId = model.FileExtensionID
            };
        }

        public static Core.DataElementDto ToSchemaDto(this DatasetFileConfigsModel model)
        {
            return new Core.DataElementDto()
            {
                DataElementName = model.ConfigFileName,
                SchemaName = model.ConfigFileName,
                SchemaDescription = model.ConfigFileDesc,
                Delimiter = model.Delimiter,
                HasHeader = model.HasHeader,
                CreateCurrentView = model.CreateCurrentView,
                FileFormatId = model.FileTypeId,
                ParentDatasetId = model.DatasetId,
                FileExtensionId = model.FileExtensionID
            };
        }
    }
}