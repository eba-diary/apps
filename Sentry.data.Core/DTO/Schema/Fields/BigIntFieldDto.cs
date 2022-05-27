using NJsonSchema;
using System.Collections.Generic;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class BigIntFieldDto : BaseFieldDto
    {
        public BigIntFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, int rowPosition, bool array) : base(prop, rowPosition, array) { }

        public BigIntFieldDto(BigIntField field) : base(field) { }

        public BigIntFieldDto(SchemaRow field) : base(field) { }

        public override string FieldType => GlobalConstants.Datatypes.BIGINT;
        public override bool Nullable { get; set; }


        //Properties that are not utilized by this type and are defaulted
        public override int Precision { get; set; }
        public override int Scale { get; set; }
        public override string SourceFormat { get; set; }
        public override int OrdinalPosition { get; set; }

        public override bool CompareToEntity(BaseField field)
        {
            bool changed = false;
            if (SchemaExtensions.TryConvertTo<BigIntField>(field) == null)
            {
                changed = true;
            }
            return changed;
        }

        public override BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision)
        {
            BaseField newEntityField = new BigIntField();
            base.ToEntity(newEntityField, parentField, parentRevision);
            return newEntityField;
        }
    }
}
