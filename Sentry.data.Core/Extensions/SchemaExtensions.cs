using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public static class SchemaExtensions
    {
        public static FileSchemaDto MapToDto(this FileSchema scm)
        {
            return new FileSchemaDto()
            {
                Name = scm.Name,
                CreateCurrentView = scm.CreateCurrentView,
                Delimiter = scm.Delimiter,
                FileExtensionId = scm.Extension.Id,
                HasHeader = scm.HasHeader,
                IsInSAS = scm.IsInSAS,
                SasLibrary = scm.SasLibrary,
                SchemaEntity_NME = scm.SchemaEntity_NME,
                SchemaId = scm.SchemaId,
                Description = scm.Description,
                DeleteInd = scm.DeleteInd,
                DeleteIssuer = scm.DeleteIssuer,
                DeleteIssueDTM = scm.DeleteIssueDTM
            };
        }

        public static SchemaRevisionDto ToDto(this SchemaRevision revision)
        {
            return new SchemaRevisionDto()
            {
                RevisionId = revision.SchemaRevision_Id,
                RevisionNumber = revision.Revision_NBR,
                SchemaRevisionName = revision.SchemaRevision_Name,
                CreatedBy = revision.CreatedBy,
                CreatedDTM = revision.CreatedDTM,
                LastUpdatedDTM = revision.LastUpdatedDTM
            };
        }

        public static BaseFieldDto ToDto(this BaseField field)
        {
            BaseFieldDto dto = new BaseFieldDto()
            {
                FieldId = field.FieldId,
                FieldGuid = field.FieldGuid,
                Name = field.Name,
                CreateDTM = field.CreateDTM,
                LastUpdatedDTM = field.LastUpdateDTM,
                FieldType = field.FieldType.ToString(),
                Nullable = field.NullableIndicator,
                OrdinalPosition = field.OrdinalPosition,
                Description = field.Description
            };

            switch (field.FieldType)
            {                
                case SchemaDatatypes.STRUCT:
                    dto.IsArray = ((StructField)field).IsArray;
                    break;
                case SchemaDatatypes.DECIMAL:
                    dto.Precision = ((DecimalField)field).Precision;
                    dto.Scale = ((DecimalField)field).Scale;
                    break;
                case SchemaDatatypes.DATE:
                    dto.SourceFormat = ((DateField)field).SourceFormat;
                    break;
                case SchemaDatatypes.TIMESTAMP:
                    dto.SourceFormat = ((TimestampField)field).SourceFormat;
                    break;
                case SchemaDatatypes.NONE:
                case SchemaDatatypes.INTEGER:
                case SchemaDatatypes.BIGINT:
                case SchemaDatatypes.VARCHAR:
                default:
                    break;
            }


            List<BaseFieldDto> childList = new List<BaseFieldDto>();
            foreach (BaseField childField in field.ChildFields)
            {
                childList.Add(childField.ToDto());
            }
            dto.ChildFields = childList;

            return dto;
        }

        public static List<BaseFieldDto> ToDto(this List<BaseField> fields)
        {
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            foreach (BaseField field in fields)
            {
                dtoList.Add(field.ToDto());
            }
            return dtoList;
        }

        public static DataElementDto ToDataElementDto(this DatasetDto dsDto)
        {
            return new DataElementDto()
            {
                DataElementName = dsDto.ConfigFileName,
                SchemaName = dsDto.ConfigFileName,
                SchemaDescription = dsDto.ConfigFileDesc,
                Delimiter = dsDto.Delimiter,
                HasHeader = dsDto.HasHeader,
                CreateCurrentView = true,
                IsInSAS = dsDto.IsInSAS,
                FileExtensionId = dsDto.FileExtensionId,
                ParentDatasetId = dsDto.DatasetId
            };
        }
    }
}
