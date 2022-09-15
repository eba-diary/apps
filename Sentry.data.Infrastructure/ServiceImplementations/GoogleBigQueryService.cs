using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using Sentry.data.Core.DTO.Schema.Fields;
using System;
using System.Collections.Generic;

namespace Sentry.data.Infrastructure
{
    public class GoogleBigQueryService : IGoogleBigQueryService
    {
        private readonly ISchemaService _schemaService;

        public GoogleBigQueryService(ISchemaService schemaService)
        {
            _schemaService = schemaService;
        }

        public void UpdateSchemaFields(int schemaId, JArray bigQueryFields)
        {
            //convert to BaseFieldDtos
            int index = 1;
            List<BaseFieldDto> fieldDtos = ConvertToFieldDtos(bigQueryFields, ref index);

            SchemaRevisionDto revision = _schemaService.GetLatestSchemaRevisionDtoBySchema(schemaId);

            if (revision != null)
            {
                //get existing fields
                List<BaseFieldDto> existingFields = _schemaService.GetBaseFieldDtoBySchemaRevision(revision.RevisionId);

                //if same, do nothing
                if (FieldsAreEqual(existingFields, fieldDtos))
                {
                    return;
                }
            }

            _schemaService.CreateAndSaveSchemaRevision(schemaId, fieldDtos, $"GoogleBigQuery_{DateTime.Today:yyyyMMdd}");
        }

        #region Private
        private List<BaseFieldDto> ConvertToFieldDtos(JArray bigQueryFields, ref int index)
        {
            List<BaseFieldDto> fieldDtos = new List<BaseFieldDto>();

            foreach (JToken bigQueryField in bigQueryFields)
            {
                fieldDtos.Add(ConvertToFieldDto(bigQueryField, ref index));
                index++;
            }

            return fieldDtos;
        }

        private BaseFieldDto ConvertToFieldDto(JToken bigQueryField, ref int index)
        {
            string dataType = bigQueryField.Value<string>("type");

            switch (dataType)
            {
                case "INTEGER":
                case "INT64":
                    return CreateField<IntegerFieldDto>(bigQueryField, index);
                case "DATE":
                    return CreateField<DateFieldDto>(bigQueryField, index);
                case "TIMESTAMP":
                case "DATETIME":
                    return CreateField<TimestampFieldDto>(bigQueryField, index);
                case "NUMERIC":
                case "BIGNUMERIC":
                    return CreateDecimalField(bigQueryField, index);
                case "RECORD":
                case "STRUCT":
                    return CreateStructField(bigQueryField, ref index);
                default:
                    return CreateField<VarcharFieldDto>(bigQueryField, index);
            }
        }

        private T CreateField<T>(JToken bigQueryField, int index) where T : BaseFieldDto
        {
            T fieldDto = Activator.CreateInstance<T>();
            fieldDto.Name = bigQueryField.Value<string>("name");
            fieldDto.IsArray = bigQueryField.Value<string>("mode") == "REPEATED";
            fieldDto.OrdinalPosition = index;

            return fieldDto;
        }

        private DecimalFieldDto CreateDecimalField(JToken bigQueryField, int index)
        {
            DecimalFieldDto fieldDto = CreateField<DecimalFieldDto>(bigQueryField, index);
            fieldDto.Precision = bigQueryField.Value<int>("precision");
            fieldDto.Scale = bigQueryField.Value<int>("scale");

            return fieldDto;
        }

        private StructFieldDto CreateStructField(JToken bigQueryField, ref int index)
        {
            StructFieldDto fieldDto = CreateField<StructFieldDto>(bigQueryField, index);
            fieldDto.HasChildren = true;
            fieldDto.ChildFields = ConvertToFieldDtos((JArray)bigQueryField.SelectToken("fields"), ref index);

            return fieldDto;
        }

        private bool FieldsAreEqual(List<BaseFieldDto> existingFields, List<BaseFieldDto> newFields)
        {
            if (existingFields.Count == newFields.Count)
            {
                for (int i = 0; i < existingFields.Count; i++)
                {
                    if (!FieldsAreEqual(existingFields[i], newFields[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private bool FieldsAreEqual(BaseFieldDto existingField, BaseFieldDto newField)
        {
            return existingField.FieldType == newField.FieldType &&
                existingField.Name == newField.Name &&
                existingField.IsArray == newField.IsArray &&
                existingField.Scale == newField.Scale &&
                existingField.Precision == newField.Precision &&
                FieldsAreEqual(existingField.ChildFields, newField.ChildFields);
        }
        #endregion
    }
}
