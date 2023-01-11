using System;
using System.Collections.Generic;
using System.Linq;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Web.Models.ApiModels.Dataset;
using Sentry.data.Web.Models.Migration;

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

        public static DatasetMigrationRequest ToDo(this DatasetMigrationRequestModel model)
        {
            DatasetMigrationRequest datasetMigrationRequest = new DatasetMigrationRequest()
            {
                SourceDatasetId = model.DatasetId,
                TargetDatasetNamedEnvironment = model.TargetNamedEnvironment
            };

            if (model.SelectedSchema.Any())
            {
                foreach(int schemaId in model.SelectedSchema)
                {
                    datasetMigrationRequest.SchemaMigrationRequests.Add(new SchemaMigrationRequest()
                    {
                        SourceSchemaId = schemaId,
                        TargetDatasetNamedEnvironment = model.TargetNamedEnvironment,
                        TargetDataFlowNamedEnvironment = model.TargetNamedEnvironment
                    });
                }
            }

            return datasetMigrationRequest;
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
                DatasetNamedEnvironment = dto.NamedEnvironment,
                Url = dto.Url
            };
        }

        public static DatasetMigrationResponseModel ToModel(this Core.DatasetMigrationRequestResponse response, IDatasetContext context)
        {
            DatasetMigrationResponseModel model = new DatasetMigrationResponseModel()
            {
                DatasetResponse = new MigrationResponseModel()
                {
                    Id = response.DatasetId,
                    WasMigrated = response.IsDatasetMigrated,
                    MigrationNotes = response.DatasetMigrationReason,
                    Name = context.Datasets.Where(w => w.DatasetId == response.DatasetId).Select(s => s.DatasetName).FirstOrDefault()
                }
            };

            if (response.SchemaMigrationResponses.Any())
            {
                foreach(var schemaResponse in response.SchemaMigrationResponses)
                {
                    _ = model.SchemaResponses.Append(new SchemaMigrationResponseModel()
                    {
                        SchemaResponse = new MigrationResponseModel()
                        {
                            Id = schemaResponse.TargetSchemaId,
                            Name = context.Schema.Where(w => w.SchemaId == schemaResponse.TargetSchemaId).Select(s => s.Name).FirstOrDefault(),
                            WasMigrated = schemaResponse.MigratedSchema,
                            MigrationNotes = schemaResponse.SchemaMigrationReason
                        },
                        SchemaRevisionResponse = new MigrationResponseModel()
                        {
                            Id = schemaResponse.TargetSchemaRevisionId,
                            Name = context.SchemaRevision.Where(w => w.SchemaRevision_Id == schemaResponse.TargetSchemaRevisionId).Select(s => s.SchemaRevision_Name).FirstOrDefault(),
                            WasMigrated = schemaResponse.MigratedSchemaRevision,
                            MigrationNotes = schemaResponse.SchemaRevisionMigrationReason
                        },
                        DataFlowResponse = new MigrationResponseModel()
                        {
                            Id = schemaResponse.TargetDataFlowId,
                            Name = context.DataFlow.Where(w => w.Id == schemaResponse.TargetDataFlowId).Select(s => s.Name).FirstOrDefault(),
                            WasMigrated = schemaResponse.MigratedDataFlow,
                            MigrationNotes = schemaResponse.DataFlowMigrationReason
                        }

                    });
                }
            }

            return model;
        }
    }
}