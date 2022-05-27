using NJsonSchema;
using System.Collections.Generic;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class IntegerFieldDto : BaseFieldDto
    {
        public IntegerFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, int rowPosition, bool array) : base(prop, rowPosition, array) { }

        public IntegerFieldDto(IntegerField field) : base(field) { }

        public IntegerFieldDto(SchemaRow field) : base(field) { }

        public override string FieldType => GlobalConstants.Datatypes.INTEGER;
        public override bool Nullable { get; set; }


        //Properties that are not utilized by this type and are defaulted
        public override int Precision { get; set; }
        public override int Scale { get; set; }
        public override string SourceFormat { get; set; }
        public override int OrdinalPosition { get; set; }

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
