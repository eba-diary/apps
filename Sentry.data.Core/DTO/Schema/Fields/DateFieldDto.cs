using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class DateFieldDto : BaseFieldDto
    {
        private int _scale;
        private int _precision;
        private int _ordinalPosition;
        private int _length;

        public DateFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, bool array) : base(prop, array)
        {
            SourceFormat = prop.Value.ExtensionData.Any(w => w.Key == "dsc-format")
                ? prop.Value.ExtensionData.Where(w => w.Key == "dsc-format").Select(s => s.Value).ToString()
                : null;
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
        public override int Precision { get { return _precision; } set { _precision = 0; } }
        public override int Scale { get { return _scale; } set { _scale = 0; } }
        public override int OrdinalPosition { get { return _ordinalPosition; } set { _ordinalPosition = 0; } }
        public override int Length { get { return _length; } set { _length = 0; } }

        public override bool CompareToEntity(BaseField field)
        {
            bool changed = false;
            if (SchemaExtensions.TryConvertTo<DateField>(field) == null) { changed = true; }
            if (changed != true && SourceFormat != ((DateField)field).SourceFormat) { changed = true; }
            return changed;
        }

        public override BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision)
        {
            BaseField newEntityField = new DateField()
            {
                //Apply defaults if neccessaryS
                SourceFormat = (string.IsNullOrWhiteSpace(this.SourceFormat)) ? GlobalConstants.Datatypes.Defaults.DATE_DEFAULT : this.SourceFormat
            };
            base.ToEntity(newEntityField, parentField, parentRevision);
            return newEntityField;
        }
    }
}
