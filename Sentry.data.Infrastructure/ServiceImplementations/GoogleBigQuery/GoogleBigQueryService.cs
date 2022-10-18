using Microsoft.Owin.Logging;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
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
            int index = 0;
            List<BaseFieldDto> fieldDtos = ConvertToFieldDtos(bigQueryFields, ref index);

            SchemaRevisionDto revision = _schemaService.GetLatestSchemaRevisionDtoBySchema(schemaId);

            if (revision != null)
            {
                //get existing fields
                List<BaseFieldDto> existingFields = _schemaService.GetBaseFieldDtoBySchemaRevision(revision.RevisionId);

                //if same, do nothing
                if (existingFields.AreEqualTo(fieldDtos))
                {
                    return;
                }
            }

            _schemaService.ValidateCleanedFields(schemaId, fieldDtos);
            _schemaService.CreateAndSaveSchemaRevision(schemaId, fieldDtos, $"GoogleBigQuery_{DateTime.Today:yyyyMMdd}");
            Logger.Info($"Google Big Query schema has been updated - SchemaId: {schemaId}");
        }

        #region Private
        private List<BaseFieldDto> ConvertToFieldDtos(JArray bigQueryFields, ref int index)
        {
            List<BaseFieldDto> fieldDtos = new List<BaseFieldDto>();

            for (int i = 0; i < bigQueryFields.Count; i++)
            {
                index++;
                JToken bigQueryField = bigQueryFields[i];
                if (i + 1 < bigQueryFields.Count && FieldsAreKeyValuePair(bigQueryField, bigQueryFields[i + 1]))
                {
                    fieldDtos.Add(CreateVarcharField(bigQueryField, index));
                    fieldDtos.Add(CreateVarcharField(bigQueryFields[i + 1], ++index));
                    i++; //skip next field
                }
                else
                {
                    fieldDtos.Add(ConvertToFieldDto(bigQueryField, ref index));
                }
            }

            return fieldDtos;
        }

        private bool FieldsAreKeyValuePair(JToken bigQueryField, JToken siblingField)
        {
            return string.Equals(bigQueryField.Value<string>("name"), "key", StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(siblingField.Value<string>("name"), "value", StringComparison.OrdinalIgnoreCase);
        }

        private BaseFieldDto ConvertToFieldDto(JToken bigQueryField, ref int index)
        {
            string dataType = bigQueryField.Value<string>("type");

            switch (dataType)
            {
                case "INTEGER":
                case "INT64":
                    return CreateField<BigIntFieldDto>(bigQueryField, index);
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
                    return CreateVarcharField(bigQueryField, index);
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

        private VarcharFieldDto CreateVarcharField(JToken bigQueryField, int index)
        {
            VarcharFieldDto fieldDto = CreateField<VarcharFieldDto>(bigQueryField, index);
            fieldDto.Length = GlobalConstants.Datatypes.Defaults.VARCHAR_LENGTH_DEFAULT;

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
        #endregion
    }
}
