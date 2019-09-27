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
                CreatedDTM = dto.CreatedDTM,
                LastUpdatedDTM = dto.LastUpdatedDTM
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

        public static Core.SchemaRow ToModel(this Core.BaseFieldDto dto)
        {
            Core.SchemaRow row = new Core.SchemaRow()
            {
                Name = dto.Name,
                Description = dto.Description,
                DataType = (dto.FieldType != null) ? dto.FieldType.ToUpper() : Core.SchemaDatatypes.VARCHAR.ToString(),
                Format = dto.SourceFormat,
                IsArray = dto.IsArray,
                Nullable = dto.Nullable,
                Position = dto.OrdinalPosition,
                Precision = dto.Precision.ToString(),
                Scale = dto.Scale.ToString(),
                LastUpdated = dto.LastUpdatedDTM.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                DataObjectField_ID = dto.FieldId
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
    }
}