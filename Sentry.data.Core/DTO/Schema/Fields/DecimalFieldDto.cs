using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class DecimalFieldDto : BaseFieldDto
    {
        public DecimalFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, bool array) : base(prop, array)
        {
            SetPrecisionScale(prop.Value.IsArray ? prop.FindArraySchema().ExtensionData : prop.Value.ExtensionData);
        }

        public DecimalFieldDto(DecimalField field) : base(field)
        {
            Nullable = field.NullableIndicator;
            Precision = field.Precision;
            Scale = field.Scale;
        }

        public DecimalFieldDto(SchemaRow row) : base(row)
        {
#pragma warning disable S3240 // The simplest possible condition syntax should be used
            if (string.IsNullOrWhiteSpace(row.Precision))
#pragma warning restore S3240 // The simplest possible condition syntax should be used
            {
                Precision = 0;
            }
            else
            {
                Precision = (Int32.TryParse(row.Precision, out int p)) ? p : 0;
            }

#pragma warning disable S3240 // The simplest possible condition syntax should be used
            if (string.IsNullOrWhiteSpace(row.Scale))
#pragma warning restore S3240 // The simplest possible condition syntax should be used
            {
                Scale = 0;
            }
            else
            {
                Scale =  (Int32.TryParse(row.Scale, out int s)) ? s : 0;
            }            
        }

        public override string FieldType => GlobalConstants.Datatypes.DECIMAL;
        public override bool Nullable { get; set; }
        public override int Precision { get; set; }
        public override int Scale { get; set; }


        //Properties that are not utilized by this type and are defaulted
        public override string SourceFormat { get; set; }
        public override int OrdinalPosition { get; set; }
        public override int Length { get; set; }

        public override BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision)
        {
            BaseField newEntityField = new DecimalField()
            {
                //Apply defaults if neccessary
                Precision = (this.Precision == 0) ? 8 : this.Precision,
                Scale = (this.Scale == 0) ? 2 : this.Scale
            };
            base.ToEntity(newEntityField, parentField, parentRevision);
            return newEntityField;
        }

        public override bool CompareToEntity(BaseField field)
        {
            bool changed = false;
            if (SchemaExtensions.TryConvertTo<DecimalField>(field) == null)
            {
                changed = true;
            }
            if (!changed && Precision != ((DecimalField)field).Precision)
            {
                changed = true;
            }
            if (!changed && Scale != ((DecimalField)field).Scale)
            {
                changed = true;
            }
            return changed;
        }

        private void SetPrecisionScale(IDictionary<string, object> extensionData)
        {
            if (extensionData != null && extensionData.Any(w => w.Key == "dsc-precision"))
            {
                var valObject = extensionData.FirstOrDefault(w => w.Key == "dsc-precision").Value;
                //handle if null was passed for dsc-scale
                string valString = (valObject != null) ? valObject.ToString() : "";
                Precision = (Int32.TryParse(valString, out int x)) ? x : 0;
            }
            else
            {
                Precision = 0;
            }

            if (extensionData != null && extensionData.Any(w => w.Key == "dsc-scale"))
            {
                var valObject = extensionData.FirstOrDefault(w => w.Key == "dsc-scale").Value;
                //handle if null was passed for dsc-scale
                string valString = (valObject != null) ? valObject.ToString() : "";
                Scale = (Int32.TryParse(valString, out int x)) ? x : 0;
            }
            else
            {
                Scale = 0;
            }
        }
    }
}
