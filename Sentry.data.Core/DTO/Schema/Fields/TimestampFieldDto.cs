using NJsonSchema;
using Sentry.Core;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class TimestampFieldDto : BaseFieldDto
    {
        public TimestampFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, int rowPosition, bool array) : base(prop, rowPosition, array)
        {
            IDictionary<string, object> extData = null;
            extData = prop.Value.IsArray ? prop.FindArraySchema().ExtensionData : prop.Value.ExtensionData;

            if (extData != null && extData.Any(w => w.Key == "dsc-format"))
            {
                object val = extData.Where(w => w.Key == "dsc-format").Select(s => s.Value).FirstOrDefault();
                SourceFormat = (val != null) ? val.ToString() : GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT;
            }
            else
            {
                SourceFormat = GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT;
            }
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

        public override ValidationResults Validate(string extension)
        {
            ValidationResults results = base.Validate(extension);
            
            if (extension == GlobalConstants.ExtensionNames.FIXEDWIDTH)
            {
                if (SourceFormat != null && Length < SourceFormat.Length)
                {
                    results.Add(OrdinalPosition.ToString(), $"({Name}) Length ({Length}) needs to be equal or greater than specified format for FIXEDWIDTH schema");
                }

                if (SourceFormat == null && Length < GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT.Length)
                {
                    results.Add(OrdinalPosition.ToString(), $"({Name}) Length ({Length}) needs to be equal or greater than default format length ({GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT.Length}) for FIXEDWIDTH schema");
                }
            }            

            return results;
        }

        public override BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision)
        {
            BaseField newEntityField = new TimestampField()
            {
                //Apply defaults if neccessary
                SourceFormat = (string.IsNullOrWhiteSpace(this.SourceFormat)) ? GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT : this.SourceFormat
            };

            newEntityField.FieldLength = ((TimestampField)newEntityField).SourceFormat.Length;

            base.ToEntity(newEntityField, parentField, parentRevision);
            return newEntityField;
        }
    }
}
