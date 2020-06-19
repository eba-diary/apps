using NJsonSchema;
using System.Collections.Generic;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class IntegerFieldDto : BaseFieldDto
    {
        private int _scale;
        private int _precision;
        private string _sourceFormat;
        private int _ordinalPosition;
        private int _length;

        public IntegerFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, bool array) : base(prop, array) { }

        public IntegerFieldDto(IntegerField field) : base(field) { }

        public IntegerFieldDto(SchemaRow field) : base(field) { }

        public override string FieldType => GlobalConstants.Datatypes.INTEGER;
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
            if (SchemaExtensions.TryConvertTo<IntegerField>(field) == null)
            {
                changed = true;
            }
            return changed;
        }

        public override BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision)
        {
            BaseField newEntityField = new IntegerField();
            base.ToEntity(newEntityField, parentField, parentRevision);
            return newEntityField;
        }
    }
}
