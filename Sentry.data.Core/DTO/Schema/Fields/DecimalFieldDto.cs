using NJsonSchema;
using Sentry.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class DecimalFieldDto : BaseFieldDto
    {
        public DecimalFieldDto() : base() { }
        public DecimalFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, int rowPosition, bool array) : base(prop, rowPosition, array)
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
            ValidationResults results = new ValidationResults();
#pragma warning disable S3240 // The simplest possible condition syntax should be used
            if (string.IsNullOrWhiteSpace(row.Precision))
#pragma warning restore S3240 // The simplest possible condition syntax should be used
            {
                Precision = GlobalConstants.Datatypes.Defaults.DECIMAL_PRECISION_DEFAULT;
            }
            else if(!Int32.TryParse(row.Precision, out int p))
            {
                results.Add(OrdinalPosition.ToString(), $"({Name}) DECIMAL Precision must be integer between 1 and 38");
            }
            else
            {
                Precision = Int32.Parse(row.Precision);
            }

#pragma warning disable S3240 // The simplest possible condition syntax should be used
            if (string.IsNullOrWhiteSpace(row.Scale))
#pragma warning restore S3240 // The simplest possible condition syntax should be used
            {
                Scale = GlobalConstants.Datatypes.Defaults.DECIMAL_SCALE_DEFAULT;
            }
            else if (!Int32.TryParse(row.Scale, out int p))
            {
                results.Add(OrdinalPosition.ToString(), $"({Name}) DECIMAL Scale must be integer between 1 and 38");
            }
            else
            {
                Scale =  (Int32.TryParse(row.Scale, out int s)) ? s : GlobalConstants.Datatypes.Defaults.DECIMAL_SCALE_DEFAULT;
            }

            if (!results.IsValid())
            {
                throw new ValidationException(results);
            }
        }

        public override string FieldType => GlobalConstants.Datatypes.DECIMAL;
        public override bool Nullable { get; set; }
        public override int Precision { get; set; }
        public override int Scale { get; set; }


        //Properties that are not utilized by this type and are defaulted
        public override string SourceFormat { get; set; }
        public override int OrdinalPosition { get; set; }

        public override BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision)
        {
            BaseField newEntityField = new DecimalField()
            {
                //Apply defaults if neccessary
                Precision = (this.Precision == 0) ? GlobalConstants.Datatypes.Defaults.DECIMAL_PRECISION_DEFAULT : this.Precision,
                Scale = this.Scale
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

        public override ValidationResults Validate(string extension)
        {
            ValidationResults results = base.Validate(extension);

            //Decimal Precision
            if (Precision < 1 || Precision > 38)
            {
                results.Add(OrdinalPosition.ToString(), $"({Name}) Precision ({Precision}) is required to be between 1 and 38");
            }

            //Decimal Scale
            if (Scale < 0 || Scale > 38)
            {
                results.Add(OrdinalPosition.ToString(), $"({Name}) Scale ({Scale}) is required to be between 0 and 38");
            }

            //Decimal Scale and Precision dependency
            if (Scale > Precision)
            {
                results.Add(OrdinalPosition.ToString(), $"({Name}) Scale ({Scale}) needs to be less than or equal to Precision ({Precision})");
            }

            if (extension == GlobalConstants.ExtensionNames.FIXEDWIDTH && Length != 0 && Length < Precision)
            {
                results.Add(OrdinalPosition.ToString(), $"({Name}) Length ({Length}) needs to be equal or greater than specified precision ({Precision}) for FIXEDWIDTH schema");
            }

            return results;
        }

        public override void Clean(string extension)
        {
            DefaultNonFixedWidthLength(extension);
        }

        private void SetPrecisionScale(IDictionary<string, object> extensionData)
        {
            if (extensionData != null && extensionData.Any(w => w.Key == "dsc-precision"))
            {
                var valObject = extensionData.FirstOrDefault(w => w.Key == "dsc-precision").Value;
                //handle if null was passed for dsc-scale
                string valString = (valObject != null) ? valObject.ToString() : "";
                Precision = (Int32.TryParse(valString, out int x)) ? x : GlobalConstants.Datatypes.Defaults.DECIMAL_PRECISION_DEFAULT;
            }
            else
            {
                Precision = GlobalConstants.Datatypes.Defaults.DECIMAL_PRECISION_DEFAULT;
            }

            if (extensionData != null && extensionData.Any(w => w.Key == "dsc-scale"))
            {
                var valObject = extensionData.FirstOrDefault(w => w.Key == "dsc-scale").Value;
                //handle if null was passed for dsc-scale
                string valString = (valObject != null) ? valObject.ToString() : "";
                Scale = (Int32.TryParse(valString, out int x)) ? x : GlobalConstants.Datatypes.Defaults.DECIMAL_SCALE_DEFAULT;
            }
            else
            {
                Scale = GlobalConstants.Datatypes.Defaults.DECIMAL_SCALE_DEFAULT;
            }
        }
    }
}
