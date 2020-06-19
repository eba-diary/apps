using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class TimestampFieldDto : BaseFieldDto
    {
        private int _scale;
        private int _precision;
        private string _sourceFormat;
        private int _ordinalPosition;
        private int _length;

        public TimestampFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, bool array) : base(prop, array)
        {
            SourceFormat = prop.Value.ExtensionData.Any(w => w.Key == "dsc-format")
                ? prop.Value.ExtensionData.Where(w => w.Key == "dsc-format").Select(s => s.Value).ToString()
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
        public override int Precision { get => _precision; set => _precision = value; }
        public override int Scale { get => _scale; set => _scale = value; }
        public override string SourceFormat { get => _sourceFormat; set => _sourceFormat = value; }
        public override int OrdinalPosition { get => _ordinalPosition; set => _ordinalPosition = value; }
        public override int Length { get => _length; set => _length = value; }

        public override bool CompareToEntity(BaseField field)
        {
            bool changed = false;
            if (SchemaExtensions.TryConvertTo<TimestampField>(field) == null) { changed = true; }
            if (changed != true && SourceFormat != ((TimestampField)field).SourceFormat) { changed = true; }
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
