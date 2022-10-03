using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Web.Models.ApiModels.Schema;
using NJsonSchema;
using Sentry.data.Core;
using System.Text;
using Sentry.data.Core.Factories.Fields;
using Sentry.Core;
using DocumentFormat.OpenXml.Spreadsheet;

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
                LastUpdatedDTM = dto.LastUpdatedDTM.ToString("s"),
                JsonSchemaObject = dto.JsonSchemaObject
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
                CreateDTM = dto.CreateDtm.ToString("s"),
                LastUpdatedDTM = dto.LastUpdatedDtm.ToString("s"),
                Nullable = dto.Nullable,
                OrdinalPosition = dto.OrdinalPosition,
                Precision = dto.Precision,
                Scale = dto.Scale,
                SourceFormat = (dto.FieldType.ToUpper() == Core.SchemaDatatypes.TIMESTAMP.ToString() || dto.FieldType.ToUpper() == Core.SchemaDatatypes.DATE.ToString()) ? SetFieldDefaults(dto.FieldType, dto.SourceFormat) : dto.SourceFormat,
                Length = dto.Length,
                DotNamePath = dto.DotNamePath,
                StructurePosition = dto.StructurePosition
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
                LastUpdated = dto.LastUpdatedDtm.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                DataObjectField_ID = dto.FieldId,
                Length = dto.Length.ToString(),
                DotNamePath = dto.DotNamePath
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
                FileExtensionName = model.FileFormat,
                HasHeader = model.HasHeader,
                CreateCurrentView = model.CurrentView,
                Description = model.Description
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaRows"></param>
        /// <exception cref="ValidationException"></exception>
        /// <returns></returns>
        public static List<BaseFieldDto> ToDto(this List<SchemaRow> schemaRows)
        {
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            foreach(SchemaRow row in schemaRows)
            {
                dtoList.Add(row.ToDto());
            }

            return dtoList;
        }

        public static BaseFieldDto ToDto(this SchemaRow row, bool convertChildren = true)
        {
            ValidationResults errors = new ValidationResults();

            if (string.IsNullOrEmpty(row.DataType))
            {
                errors.Add(row.Position.ToString(), "Must select datatype");

                throw new ValidationException(errors);
            }

            FieldDtoFactory fieldFactory;
            switch (row.DataType.ToUpper())
            {
                case GlobalConstants.Datatypes.DECIMAL:
                    fieldFactory = new DecimalFieldDtoFactory(row);
                    break;
                case GlobalConstants.Datatypes.DATE:
                    fieldFactory = new DateFieldDtoFactory(row);
                    break;
                case GlobalConstants.Datatypes.INTEGER:
                    fieldFactory = new IntegerFieldDtoFactory(row);
                    break;
                case GlobalConstants.Datatypes.STRUCT:
                    fieldFactory = new StructFieldDtoFactory(row);
                    break;
                case GlobalConstants.Datatypes.TIMESTAMP:
                    fieldFactory = new TimestampFieldDtoFactory(row);
                    break;
                case GlobalConstants.Datatypes.VARCHAR:
                    fieldFactory = new VarcharFieldDtoFactory(row);
                    break;
                case GlobalConstants.Datatypes.BIGINT:
                    fieldFactory = new BigIntFieldDtoFactory(row);
                    break;
                default:
                    fieldFactory = new VarcharFieldDtoFactory(row);
                    break;
            }

            BaseFieldDto dto = fieldFactory.GetField();

            if (row.ChildRows != null && row.ChildRows.Any())
            {
                dto.HasChildren = true;

                if (convertChildren)
                {
                    foreach (SchemaRow childrow in row.ChildRows)
                    {
                        dto.ChildFields.Add(childrow.ToDto());
                    }
                }
            }

            return dto;            
        }

        public static SchemaRevisionJsonStructureModel ToModel(this SchemaRevisionJsonStructureDto dto)
        {
            SchemaRevisionJsonStructureModel mdl = new SchemaRevisionJsonStructureModel()
            {
                Revision = dto.Revision?.ToModel(),
                JsonStructure = dto.JsonStructure
            };

            //setting to null because would be duplicating what is in JsonStructure
            if (mdl.Revision != null)
            {
                mdl.Revision.JsonSchemaObject = null;
            }

            return mdl;
        }
    }
}