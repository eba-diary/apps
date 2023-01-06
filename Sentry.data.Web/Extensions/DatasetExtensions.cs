using System;
using System.Collections.Generic;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Models.ApiModels.Dataset;


namespace Sentry.data.Web
{
    public static class DatasetExtensions
    {
        public static DatasetSchemaDto ToDto(this DatasetModel model)
        {
            if (model == null) { return new DatasetSchemaDto(); }

            return new DatasetSchemaDto()
            {
                DatasetId = model.DatasetId,
                DatasetCategoryIds = model.DatasetCategoryIds,
                DatasetName = model.DatasetName,
                ShortName = model.ShortName,
                DatasetDesc = model.DatasetDesc,
                DatasetInformation = model.DatasetInformation,
                PrimaryContactId = model.PrimaryContactId,
                PrimaryContactName = model.PrimaryContactName,
                AlternateContactEmail = model.AlternateContactEmail,
                CreationUserId = model.CreationUserId,
                UploadUserId = model.UploadUserId,
                DatasetDtm = DateTime.Now,
                ChangedDtm = DateTime.Now,
                OriginationId = model.OriginationID,
                ConfigFileName = model.ConfigFileName,
                ConfigFileDesc = model.ConfigFileDesc,
                FileExtensionId = model.FileExtensionId,
                Delimiter = model.Delimiter,
                SchemaRootPath = model.SchemaRootPath,
                DatasetScopeTypeId = model.DatasetScopeTypeId,
                DataClassification = model.DataClassification,
                IsSecured = model.IsSecured,
                HasHeader = model.HasHeader,
                CreateCurrentView = model.CreateCurrentView,
                ObjectStatus = model.ObjectStatus,
                SAIDAssetKeyCode = model.SAIDAssetKeyCode,
                NamedEnvironment = model.DatasetNamedEnvironment,
                NamedEnvironmentType = model.DatasetNamedEnvironmentType
            };
        }

        public static List<DatasetInfoModel> ToApiModel(this List<DatasetSchemaDto> dtoList)
        {
            List<DatasetInfoModel> modelList = new List<DatasetInfoModel>();
            foreach (DatasetSchemaDto dto in dtoList)
            {                
                modelList.Add(dto.ToApiModel());
            }
            return modelList;
        }

        public static DatasetInfoModel ToApiModel(this DatasetSchemaDto dto)
        {
            return new DatasetInfoModel()
            {
                Id = dto.DatasetId,
                Name = dto.DatasetName,
                ShortName = dto.ShortName,
                Category = dto.CategoryName,
                Description = dto.DatasetDesc,
                IsSecure = dto.IsSecured,
                PrimaryContactName = dto.PrimaryContactName,
                PrimarContactEmail = dto.PrimaryContactEmail,
                AlternateContactEmail = dto.AlternateContactEmail,
                ObjectStatus = dto.ObjectStatus.GetDescription().ToUpper(),
                SAIDAssetKeyCode = dto.SAIDAssetKeyCode,
                NamedEnvironment = dto.NamedEnvironment,
                NamedEnvironmentType = dto.NamedEnvironmentType
            };
        }

        public static DatasetFileConfigSchemaModel ToModel(this DatasetFileConfigSchemaDto dto)
        {
            return new DatasetFileConfigSchemaModel()
            {
                ConfigId = dto.ConfigId,
                SchemaId = dto.SchemaId,
                SchemaName = dto.SchemaName
            };
        }

        public static DatasetRelativeModel ToModel(this DatasetRelativeDto dto)
        {
            return new DatasetRelativeModel()
            {
                DatasetId = dto.DatasetId,
                NamedEnvironment = dto.NamedEnvironment,
                Url = dto.Url
            };
        }
    }
}