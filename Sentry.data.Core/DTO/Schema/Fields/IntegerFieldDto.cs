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
        public override int Precision { get { return _precision; } set { _precision = 0; } }
        public override int Scale { get { return _scale; } set { _scale = 0; } }
        public override string SourceFormat { get { return _sourceFormat; } set { _sourceFormat = null; } }
        public override int OrdinalPosition { get { return _ordinalPosition; } set { _ordinalPosition = 0; } }
        public override int Length { get { return _length; } set { _length = 0; } }

        public override bool CompareToEntity(BaseField field)
        {
            bool changed = false;
            if (SchemaExtensions.TryConvertTo<IntegerField>(field) == null) { changed = true; }
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
