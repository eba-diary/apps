using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class DecimalFieldDto : BaseFieldDto
    {
        private string _sourceFormat;
        private int _ordinalPosition;
        private int _length;

        public DecimalFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, bool array) : base(prop, array)
        {

            if (prop.Value.ExtensionData != null && prop.Value.ExtensionData.Any(w => w.Key == "dsc-precision"))
            {
                var valObject = prop.Value.ExtensionData.FirstOrDefault(w => w.Key == "dsc-precision").Value;
                //handle if null was passed for dsc-scale
                string valString = (valObject != null) ? valObject.ToString() : "";
                Precision = (Int32.TryParse(valString, out int x)) ? x : 0;
            }
            else
            {
                Precision = 0;
            }

            if (prop.Value.ExtensionData != null && prop.Value.ExtensionData.Any(w => w.Key == "dsc-scale"))
            {
                var valObject = prop.Value.ExtensionData.FirstOrDefault(w => w.Key == "dsc-scale").Value;
                //handle if null was passed for dsc-scale
                string valString = (valObject != null) ? valObject.ToString() : "";
                Scale = (Int32.TryParse(valString, out int x)) ? x : 0;
            }
            else
            {
                Scale = 0;
            }
        }

        public DecimalFieldDto(DecimalField field) : base(field)
        {
            Nullable = field.NullableIndicator;
            Precision = field.Precision;
            Scale = field.Scale;
        }

        public DecimalFieldDto(SchemaRow row) : base(row)
        {
            Precision = string.IsNullOrWhiteSpace(row.Precision) ? 0 : (Int32.TryParse(row.Precision, out int p)) ? p : 0;

            Scale = string.IsNullOrWhiteSpace(row.Scale) ? 0 : (Int32.TryParse(row.Scale, out int s)) ? s : 0;
        }

        public override string FieldType => GlobalConstants.Datatypes.DECIMAL;
        public override bool Nullable { get; set; }
        public override int Precision { get; set; }
        public override int Scale { get; set; }


        //Properties that are not utilized by this type and are defaulted
        public override string SourceFormat { get => _sourceFormat; set => _sourceFormat = value; }
        public override int OrdinalPosition { get => _ordinalPosition; set => _ordinalPosition = value; }
        public override int Length { get => _length; set => _length = value; }

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
    }
}
