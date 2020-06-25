using NJsonSchema;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class TimestampFieldDto : BaseFieldDto
    {
        public TimestampFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, bool array) : base(prop, array)
        {
            IDictionary<string, object> extData = null;
            extData = prop.Value.IsArray ? prop.FindArraySchema().ExtensionData : prop.Value.ExtensionData;

            SourceFormat = extData.Any(w => w.Key == "dsc-format")
                ? extData.Where(w => w.Key == "dsc-format").Select(s => s.Value).FirstOrDefault().ToString()
                : null;
        }

        public TimestampFieldDto(TimestampField field) : base(field)
        {
            SourceFormat = field.SourceFormat;
        }

        public TimestampFieldDto(SchemaRow row) : base(row)
        {
            SourceFormat = row.Format;
        }

        public override string FieldType => GlobalConstants.Datatypes.TIMESTAMP;
        public override bool Nullable { get; set; }


        //Properties that are not utilized by this type and are defaulted
        public override int Precision { get; set; }
        public override int Scale { get; set; }
        public override string SourceFormat { get; set; }
        public override int OrdinalPosition { get; set; }
        public override int Length { get; set; }

        public override bool CompareToEntity(BaseField field)
        {
            bool changed = false;
            if (SchemaExtensions.TryConvertTo<TimestampField>(field) == null)
            {
                changed = true;
            }
            if (!changed && SourceFormat != ((TimestampField)field).SourceFormat)
            {
                changed = true;
            }
            return changed;
        }

        public override BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision)
        {
            BaseField newEntityField = new TimestampField()
            {
                //Apply defaults if neccessary
                SourceFormat = (string.IsNullOrWhiteSpace(this.SourceFormat)) ? GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT : this.SourceFormat
            };
            base.ToEntity(newEntityField, parentField, parentRevision);
            return newEntityField;
        }
    }
}
