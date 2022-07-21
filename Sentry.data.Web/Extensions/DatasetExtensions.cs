using System;
using System.Collections.Generic;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Models.ApiModels.Dataset;


namespace Sentry.data.Web
{
    public static class DatasetExtensions
    {
        public static DatasetDto ToDto(this DatasetModel model)
        {
            if (model == null) { return new DatasetDto(); }

            return new DatasetDto()
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
                NamedEnvironment = model.NamedEnvironment,
                NamedEnvironmentType = model.NamedEnvironmentType
            };
        }

        public static List<DatasetInfoModel> ToApiModel(this List<DatasetDto> dtoList)
        {
            List<DatasetInfoModel> modelList = new List<DatasetInfoModel>();
            foreach (DatasetDto dto in dtoList)
            {                
                modelList.Add(dto.ToApiModel());
            }
            return modelList;
        }

        public static DatasetInfoModel ToApiModel(this DatasetDto dto)
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

        public static DatasetSearchDto ToDto(this DatasetSearchModel model)
        {
            DatasetSearchDto dto = new DatasetSearchDto()
            {
                PageSize = model.PageSize,
                PageNumber = model.PageNumber
            };

            switch ((DatasetSortByOption)model.SortBy)
            {
                case DatasetSortByOption.Alphabetical:
                    dto.OrderByField = x => x.Name;
                    break;
                case DatasetSortByOption.Favorites:
                    dto.OrderByField = x => x.IsFavorite;
                    dto.OrderByDescending = true;
                    break;
                case DatasetSortByOption.MostAccessed:
                    dto.OrderByField = x => x.PageViews;
                    dto.OrderByDescending = true;
                    break;
                case DatasetSortByOption.RecentlyAdded:
                    dto.OrderByField = x => x.CreatedDateTime;
                    dto.OrderByDescending = true;
                    break;
                case DatasetSortByOption.RecentlyUpdated:
                    dto.OrderByField = x => x.LastUpdated;
                    dto.OrderByDescending = true;
                    break;
                default:
                    break;
            }

            return dto;
        }
    }
}