using NJsonSchema;
using Sentry.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class DateFieldDto : BaseFieldDto
    {
        public DateFieldDto() : base() { }
        public DateFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, int rowPosition, bool array) : base(prop, rowPosition, array)
        {
            IDictionary<string, object> extData = null;
            extData = prop.Value.IsArray ? prop.FindArraySchema().ExtensionData : prop.Value.ExtensionData;

            if (extData != null && extData.Any(w => w.Key == "dsc-format"))
            {
                object val = extData.Where(w => w.Key == "dsc-format").Select(s => s.Value).FirstOrDefault();
                SourceFormat = (val != null) ? val.ToString() : GlobalConstants.Datatypes.Defaults.DATE_DEFAULT;
            }
            else
            {
                SourceFormat = GlobalConstants.Datatypes.Defaults.DATE_DEFAULT;
            }
        }

        public DateFieldDto(DateField field) : base(field)
        {
            SourceFormat = field.SourceFormat;
        }

        public DateFieldDto(SchemaRow row) : base(row)
        {
            SourceFormat = row.Format;
        }


        public override string FieldType => GlobalConstants.Datatypes.DATE;
        public override bool Nullable { get; set; }
        public override string SourceFormat { get; set; }


        //Properties that are not utilized by this type and are defaulted
        public override int Precision { get; set; }
        public override int Scale { get; set; }
        public override int OrdinalPosition { get; set; }

        public override bool CompareToEntity(BaseField field)
        {
            bool changed = false;
            if (SchemaExtensions.TryConvertTo<DateField>(field) == null)
            {
                changed = true;
            }

            if (!changed && SourceFormat != ((DateField)field).SourceFormat)
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
                
                if (SourceFormat == null && Length < GlobalConstants.Datatypes.Defaults.DATE_DEFAULT.Length)
                {
                    results.Add(OrdinalPosition.ToString(), $"({Name}) Length ({Length}) needs to be equal or greater than default format length ({GlobalConstants.Datatypes.Defaults.DATE_DEFAULT.Length}) for FIXEDWIDTH schema");
                }
            }

            return results;
        }

        public override BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision)
        {
            BaseField newEntityField = new DateField()
            {
                //Apply defaults if neccessary
                SourceFormat = (string.IsNullOrWhiteSpace(this.SourceFormat)) ? GlobalConstants.Datatypes.Defaults.DATE_DEFAULT : this.SourceFormat
            };

            newEntityField.FieldLength = ((DateField)newEntityField).SourceFormat.Length;

            base.ToEntity(newEntityField, parentField, parentRevision);
            return newEntityField;
        }

        public override void Clean(string extension)
        {
            DefaultNonFixedWidthLength(extension);
        }
    }
}
