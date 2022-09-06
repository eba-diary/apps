using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Helpers;
using Sentry.data.Web.Models.ApiModels.Config;
using Sentry.data.Web.Models.ApiModels.Schema;
using System;
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
            dto.ParentDatasetId = model.DatasetId;
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
                ObjectStatus = model.ObjectStatus,
                SchemaRootPath = model.SchemaRootPath
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
                HasHeader = model.HasHeader,
                Delimiter = model.Delimiter,
                CreateCurrentView = model.CreateCurrentView,
                ObjectStatus = model.ObjectStatus,
                SchemaRootPath = model.SchemaRootPath,
                ParquetStorageBucket = model.ParquetStorageBucket,
                ParquetStoragePrefix = model.ParquetStoragePrefix,
                ControlMTriggerName = model.ControlMTriggerName
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
                SchemaRootPath = model.SchemaRootPath,
                ParquetStorageBucket = model.ParquetStorageBucket,
                ParquetStoragePrefix = model.ParquetStoragePrefix,
                ControlMTriggerName = model.ControlMTriggerName
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
                SasLibrary = model.SasLibrary,
                ControlMTriggerName = model.ControlMTriggerName
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
            var model = new SchemaInfoModel();
            model.PopulateSchemaModelBase(dto);

            //Flatten the consumption details down to a single Snowflake consumption detail, as this API expects
            //If the old "Category" schema parquet snowflake consumption data exists, return that
            var consumptionDetail = dto.Schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflakeDto>().FirstOrDefault(s => s.SnowflakeType == SnowflakeConsumptionType.CategorySchemaParquet);
            //if the old doesn't exist, then return the new
            if (consumptionDetail == null)
            {
                consumptionDetail = dto.Schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflakeDto>().FirstOrDefault(s => s.SnowflakeType == SnowflakeConsumptionType.DatasetSchemaParquet);
            }

            model.SnowflakeDatabase = consumptionDetail.SnowflakeDatabase;
            model.SnowflakeSchema = consumptionDetail.SnowflakeSchema;
            model.SnowflakeTable = consumptionDetail.SnowflakeTable;
            model.SnowflakeStatus = consumptionDetail.SnowflakeStatus;
            model.SnowflakeStage = consumptionDetail.SnowflakeStage;
            model.SnowflakeWarehouse = consumptionDetail.SnowflakeWarehouse;

            return model;
        }

        public static Models.ApiModels.Schema20220609.SchemaInfoModel ToSchemaModel20220609(this Core.DatasetFileConfigDto dto)
        {
            var model = new Models.ApiModels.Schema20220609.SchemaInfoModel();
            model.PopulateSchemaModelBase(dto);
            model.ConsumptionDetails = dto.Schema.ConsumptionDetails.Select(c => c.Accept(new Models.ApiModels.Schema20220609.SchemaConsumptionModelTransformer())).ToList();
            return model;
        }

        private static void PopulateSchemaModelBase(this SchemaInfoModelBase model, Core.DatasetFileConfigDto dto)
        {
            Core.FileSchemaDto schemaDto = dto.Schema;
            model.ConfigId = dto.ConfigId;
            model.Name = schemaDto.Name;
            model.SchemaId = schemaDto.SchemaId;
            model.SchemaEntity_NME = schemaDto.SchemaEntity_NME;
            model.Description = schemaDto.Description;
            model.StorageCode = schemaDto.StorageCode;
            model.Format = schemaDto.FileExtensionName;
            model.CurrentView = schemaDto.CreateCurrentView;
            model.Delimiter = schemaDto.Delimiter;
            model.HasHeader = schemaDto.HasHeader;
            model.IsTrackableSchema = dto.IsTrackableSchema;
            model.HiveTable = schemaDto.HiveTable;
            model.HiveDatabase = schemaDto.HiveDatabase;
            model.HiveTableStatus = schemaDto.HiveStatus;
            model.HiveLocation = schemaDto.HiveLocation;
            model.Options = new List<string>()
            {
                "CLA1396_NewEtlColumns|" + schemaDto.CLA1396_NewEtlColumns.ToString(),
                "CLA1580_StructureHive|" + schemaDto.CLA1580_StructureHive.ToString(),
                "CLA2472_EMRSend|" + schemaDto.CLA2472_EMRSend.ToString(),
                "CLA1286_KafkaFlag|" + schemaDto.CLA1286_KafkaFlag.ToString(),
                "CLA3014_LoadDataToSnowflake|" + schemaDto.CLA3014_LoadDataToSnowflake.ToString()
            };
            model.DeleteInd = schemaDto.DeleteInd;
            model.ObjectStatus = schemaDto.ObjectStatus.GetDescription().ToUpper();
            model.SchemaRootPath = schemaDto.SchemaRootPath?.Split(',');
            model.HasDataFlow = dto.HasDataFlow;
            model.ParquetStorageBucket = schemaDto.ParquetStorageBucket;
            model.ParquetStoragePrefix = schemaDto.ParquetStoragePrefix;
            model.ControlMTriggerName = schemaDto.ControlMTriggerName;
        }

        public static List<SchemaInfoModel> ToSchemaModel(this List<Core.DatasetFileConfigDto> dtoList)
        {
            return dtoList.Select(dto => dto.ToSchemaModel()).ToList();
        }

        public static List<Models.ApiModels.Schema20220609.SchemaInfoModel> ToSchemaModel20220609(this List<Core.DatasetFileConfigDto> dtoList)
        {
            return dtoList.Select(dto => dto.ToSchemaModel20220609()).ToList();
        }

        public static FileSchemaDto ToDto(this SchemaInfoModel mdl, int datasetId, Func<string, int> extIdLookup)
        {
            var dto = ((SchemaInfoModelBase)mdl).ToDto(datasetId, extIdLookup);
            //the old SchemaInfoModel doesn't hold the SchemaConsumptionId - so we just put the data we have into a SchemaConsumptionSnowflakeDto as-is -
            //it will be up to the SchemaService implementation to translate this to the correct SchemaConsumption record
            dto.ConsumptionDetails = new List<SchemaConsumptionDto>() {
                    new SchemaConsumptionSnowflakeDto()
                    {
                        SnowflakeDatabase = mdl.SnowflakeDatabase,
                        SnowflakeSchema = mdl.SnowflakeSchema,
                        SnowflakeTable = mdl.SnowflakeTable,
                        SnowflakeStatus = mdl.SnowflakeStatus,
                        SnowflakeStage = mdl.SnowflakeStage,
                        SnowflakeWarehouse = mdl.SnowflakeWarehouse,
                        SnowflakeType = SnowflakeConsumptionType.CategorySchemaParquet //assume that the legacy SchemaInfoModel is referring to the legacy Snowflake type
                    }
                };
            return dto;
        }

        public static FileSchemaDto ToDto(this Models.ApiModels.Schema20220609.SchemaInfoModel mdl, int datasetId, Func<string, int> extIdLookup)
        {
            var dto = ((SchemaInfoModelBase)mdl).ToDto(datasetId, extIdLookup);
            dto.ConsumptionDetails = mdl.ConsumptionDetails.Select(c => c.Accept(new Models.ApiModels.Schema20220609.SchemaConsumptionModelTransformer())).ToList();
            return dto;
        }

        private static FileSchemaDto ToDto(this SchemaInfoModelBase mdl, int datasetId, Func<string, int> extIdLookup)
        {
            return new FileSchemaDto()
            {
                SchemaId = mdl.SchemaId,
                SchemaEntity_NME = mdl.SchemaEntity_NME,
                Name = mdl.Name,
                Description = mdl.Description,
                ObjectStatus = EnumHelper.GetByDescription<ObjectStatusEnum>(mdl.ObjectStatus),
                DeleteInd = mdl.DeleteInd,
                ParentDatasetId = datasetId,
                CLA1396_NewEtlColumns = mdl.Options?.Any(a => string.Equals(a, "CLA1396_NewEtlColumns|true", StringComparison.OrdinalIgnoreCase)) == true,
                CLA1580_StructureHive = mdl.Options?.Any(a => string.Equals(a, "CLA1580_StructureHive|true", StringComparison.OrdinalIgnoreCase)) == true,
                CLA2472_EMRSend = mdl.Options?.Any(a => string.Equals(a, "CLA2472_EMRSend|true", StringComparison.OrdinalIgnoreCase)) == true,
                CLA1286_KafkaFlag = mdl.Options?.Any(a => string.Equals(a, "CLA1286_KafkaFlag|true", StringComparison.OrdinalIgnoreCase)) == true,
                CLA3014_LoadDataToSnowflake = mdl.Options?.Any(a => string.Equals(a, "CLA3014_LoadDataToSnowflake|true", StringComparison.OrdinalIgnoreCase)) == true,
                FileExtensionId = extIdLookup(mdl.Format),
                FileExtensionName = mdl.Format,
                Delimiter = mdl.Delimiter,
                HasHeader = mdl.HasHeader,
                CreateCurrentView = mdl.CurrentView,
                HiveTable = mdl.HiveTable,
                HiveDatabase = mdl.HiveDatabase,
                HiveLocation = mdl.HiveLocation,
                HiveStatus = mdl.HiveTableStatus,
                StorageCode = mdl.StorageCode,
                SchemaRootPath = mdl.SchemaRootPath != null ? string.Join(",", mdl.SchemaRootPath) : null,
                ParquetStorageBucket = mdl.ParquetStorageBucket,
                ParquetStoragePrefix = mdl.ParquetStoragePrefix,
                ControlMTriggerName = mdl.ControlMTriggerName
            };
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
            model.Schema = config.Schema ?? null;
            model.RawStorageId = config.Schema.StorageCode;
            model.Delimiter = config.Schema?.Delimiter;
            model.SchemaId = (config.Schema != null) ? config.Schema.SchemaId : 0;
            model.CreateCurrentView = (config.Schema != null) ? config.Schema.CreateCurrentView : false;
            model.HasHeader = (config.Schema != null) ? config.Schema.HasHeader : false;
            model.CLA1396_NewEtlColumns = (config.Schema != null) ? config.Schema.CLA1396_NewEtlColumns : false;
            model.CLA1580_StructureHive = (config.Schema != null) ? config.Schema.CLA1580_StructureHive : false;
            model.CLA2472_EMRSend = (config.Schema != null) ? config.Schema.CLA2472_EMRSend : false;
            model.CLA1286_KafkaFlag = (config.Schema != null) ? config.Schema.CLA1286_KafkaFlag : false;
            model.CLA3014_LoadDataToSnowflake = (config.Schema != null) ? config.Schema.CLA3014_LoadDataToSnowflake : false;
            model.DeleteInd = config.Schema.DeleteInd;
            model.ObjectStatus = config.ObjectStatus;
            model.SchemaRootPath = config.Schema?.SchemaRootPath;
            model.ObjectStatus = config.ObjectStatus;
            model.ParquetStorageBucket = config.Schema?.ParquetStorageBucket;
            model.ParquetStoragePrefix = config.Schema?.ParquetStoragePrefix;
            model.ControlMTriggerName = config.Schema?.ControlMTriggerName;
        }
    }
}