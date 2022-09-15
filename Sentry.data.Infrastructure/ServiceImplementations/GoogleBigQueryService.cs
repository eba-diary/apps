using Newtonsoft.Json.Linq;
using Sentry.data.Core;
using Sentry.data.Core.DTO.Schema.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

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
            //convert to List of BaseFieldDto
            int index = 1;
            List<BaseFieldDto> fieldDtos = MapToFieldDtos(bigQueryFields, ref index);

            SchemaRevisionDto revision = _schemaService.GetLatestSchemaRevisionDtoBySchema(schemaId);

            if (revision == null)
            {
                //add new
                _schemaService.CreateAndSaveSchemaRevision(schemaId, fieldDtos, $"GoogleBigQuery_{DateTime.Today:yyyyMMdd}");
            }
            else
            {
                //compare
                List<BaseFieldDto> existingFields = _schemaService.GetBaseFieldDtoBySchemaRevision(revision.RevisionId);

                //if same, do nothing


                //if different, add new revision
            }
        }

        #region Private
        private List<BaseFieldDto> MapToFieldDtos(JArray bigQueryFields, ref int index)
        {
            List<BaseFieldDto> fieldDtos = new List<BaseFieldDto>();

            foreach (JToken bigQueryField in bigQueryFields)
            {
                fieldDtos.Add(MapToFieldDto(bigQueryField, ref index));
                index++;
            }

            return fieldDtos;
        }

        private BaseFieldDto MapToFieldDto(JToken bigQueryField, ref int index)
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
            fieldDto.ChildFields = MapToFieldDtos((JArray)bigQueryField.SelectToken("fields"), ref index);

            return fieldDto;
        }
        #endregion
    }
}
