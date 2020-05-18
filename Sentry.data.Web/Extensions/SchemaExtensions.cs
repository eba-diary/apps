using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Web.Models.ApiModels.Schema;


namespace Sentry.data.Web
{
    public static class SchemaExtensions
    {
        public static SchemaInfoModel ToModel(this Core.SchemaDto dto)
        {
            return new SchemaInfoModel()
            {
                SchemaId = dto.SchemaId,
                SchemaEntity_NME = dto.SchemaEntity_NME,
                Name = dto.Name
            };
        }

        public static SchemaRevisionModel ToModel(this Core.SchemaRevisionDto dto)
        {
            return new SchemaRevisionModel()
            {
                RevisionId = dto.RevisionId,
                RevisionNumber = dto.RevisionNumber,
                SchemaRevisionName = dto.SchemaRevisionName,
                CreatedBy = dto.CreatedBy,
                CreatedByName = dto.CreatedByName,
                CreatedDTM = dto.CreatedDTM.ToString("s"),
                LastUpdatedDTM = dto.LastUpdatedDTM.ToString("s")
            };
        }

        public static List<SchemaRevisionModel> ToModel(this List<Core.SchemaRevisionDto> dtoList)
        {
            List<SchemaRevisionModel> modelList = new List<SchemaRevisionModel>();
            foreach(Core.SchemaRevisionDto dto in dtoList)
            {
                modelList.Add(dto.ToModel());
            }
            return modelList;
        }

        public static SchemaRevisionDetailModel ToSchemaDetailModel(this Core.SchemaRevisionDto dto)
        {
            return new SchemaRevisionDetailModel()
            {
                Revision = dto.ToModel()
            };
        }

        public static SchemaFieldModel ToSchemaFieldModel(this Core.BaseFieldDto dto)
        {
            SchemaFieldModel model = new SchemaFieldModel()
            {
                Name = dto.Name,
                Description = dto.Description,
                FieldGuid = dto.FieldGuid,
                FieldType = dto.FieldType,
                IsArray = dto.IsArray,
                CreateDTM = dto.CreateDTM.ToString("s"),
                LastUpdatedDTM = dto.LastUpdatedDTM.ToString("s"),
                Nullable = dto.Nullable,
                OrdinalPosition = dto.OrdinalPosition,
                Precision = dto.Precision,
                Scale = dto.Scale,
                SourceFormat = (dto.FieldType.ToUpper() == Core.SchemaDatatypes.TIMESTAMP.ToString() || dto.FieldType.ToUpper() == Core.SchemaDatatypes.DATE.ToString()) ? SetFieldDefaults(dto.FieldType, dto.SourceFormat) : dto.SourceFormat
            };

            if (dto.ChildFields.Any())
            {
                List<SchemaFieldModel> childList = new List<SchemaFieldModel>();
                foreach(Core.BaseFieldDto childDto in dto.ChildFields)
                {
                    childList.Add(childDto.ToSchemaFieldModel());
                }

                model.Fields = childList;
            }
            return model;
        }

        private static string SetFieldDefaults(string fieldType, string sourceFormat)
        {
            if (fieldType.ToUpper() == Core.SchemaDatatypes.DATE.ToString().ToUpper())
            {
                return (sourceFormat != null) ? sourceFormat : Core.GlobalConstants.Datatypes.Defaults.DATE_DEFAULT;
            }
            else if(fieldType.ToUpper() == Core.SchemaDatatypes.TIMESTAMP.ToString().ToUpper())
            {
                return (sourceFormat != null) ? sourceFormat : Core.GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT;
            }
            else
            {
                return sourceFormat;
            }
        }

        public static List<SchemaFieldModel> ToSchemaFieldModel(this List<Core.BaseFieldDto> fieldDtoList)
        {
            List<SchemaFieldModel> modelList = new List<SchemaFieldModel>();
            foreach(Core.BaseFieldDto dto in fieldDtoList)
            {
                modelList.Add(dto.ToSchemaFieldModel());
            }
            return modelList;
        }

        public static Core.SchemaRow ToModel(this Core.BaseFieldDto dto)
        {
            Core.SchemaRow row = new Core.SchemaRow()
            {
                Name = dto.Name,
                FieldGuid = dto.FieldGuid,
                Description = dto.Description,
                DataType = (dto.FieldType != null) ? dto.FieldType.ToUpper() : Core.SchemaDatatypes.VARCHAR.ToString(),
                Format = dto.SourceFormat,
                IsArray = dto.IsArray,
                Nullable = dto.Nullable,
                Position = dto.OrdinalPosition,
                Precision = dto.Precision.ToString(),
                Scale = dto.Scale.ToString(),
                LastUpdated = dto.LastUpdatedDTM.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                DataObjectField_ID = dto.FieldId,
                Length = dto.Length.ToString()
            };

            if (dto.ChildFields.Any())
            {
                List<Core.SchemaRow> childRows = new List<Core.SchemaRow>();
                foreach (Core.BaseFieldDto childDto in dto.ChildFields)
                {
                    childRows.Add(childDto.ToModel());
                }
                row.ChildRows = childRows;
            }
            else
            {
                row.ChildRows = new List<Core.SchemaRow>();
            }

            return row;
        }

        public static Core.FileSchemaDto ToDto(this CreateSchemaModel model)
        {
            return new Core.FileSchemaDto()
            {
                Name = model.Name,
                Delimiter = model.Delimiter,
                FileExtenstionName = model.FileFormat,
                HasHeader = model.HasHeader,
                IsInSAS = model.AddToSAS,
                CreateCurrentView = model.CurrentView,
                Description = model.Description
            };
        }
    }
}