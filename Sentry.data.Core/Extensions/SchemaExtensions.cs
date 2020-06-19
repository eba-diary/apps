using Sentry.data.Core.Factories.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public static class SchemaExtensions
    {

        public static SchemaRevisionDto ToDto(this SchemaRevision revision)
        {
            return new SchemaRevisionDto()
            {
                RevisionId = revision.SchemaRevision_Id,
                RevisionNumber = revision.Revision_NBR,
                SchemaRevisionName = revision.SchemaRevision_Name,
                CreatedBy = revision.CreatedBy,
                CreatedDTM = revision.CreatedDTM,
                LastUpdatedDTM = revision.LastUpdatedDTM,
                JsonSchemaObject = revision.JsonSchemaObject
            };
        }

        public static BaseFieldDto ToDto(this BaseField field)
        {
            FieldDtoFactory factory;
            BaseFieldDto dto = null;

            switch (field)
            {                 
                case DecimalField dm:
                    factory = new DecimalFieldDtoFactory(dm);
                    break;
                case DateField dt:
                    factory = new DateFieldDtoFactory(dt);
                    break;
                case TimestampField tm:
                    factory = new TimestampFieldDtoFactory(tm);
                    break;
                case VarcharField v:
                    factory = new VarcharFieldDtoFactory(v);
                    break;
                case IntegerField i:
                    factory = new IntegerFieldDtoFactory(i);
                    break;
                case StructField s:
                    factory = new StructFieldDtoFactory(s);
                    break;
                case BigintField _:
                default:
                    factory = null;
                    break;
            }

            if (factory != null)
            {
                dto = factory.GetField();

                List<BaseFieldDto> childList = new List<BaseFieldDto>();
                foreach (BaseField childField in field.ChildFields)
                {
                    childList.Add(childField.ToDto());
                }
                dto.ChildFields = childList;
            }

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
        public static Object TryConvertTo<T>(Object input)
        {
            Object result = null;
            try
            {
                result = Convert.ChangeType(input, typeof(T));
            }
            catch
            {
            }

            return result;
        }
    }
}
