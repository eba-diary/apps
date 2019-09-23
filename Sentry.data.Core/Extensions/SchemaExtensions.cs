using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public static class SchemaExtensions
    {
        public static SchemaDto MapToSchemaDto(this Schema scm)
        {
            return new SchemaDto()
            {
                SchemaId = scm.SchemaId,
                Name = scm.Name,
                SchemaEntity_NME = scm.SchemaEntity_NME
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
                Name = field.Name,
                CreateDTM = field.CreateDTM,
                FieldType = field.FieldType.ToString()
            };
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
    }
}
