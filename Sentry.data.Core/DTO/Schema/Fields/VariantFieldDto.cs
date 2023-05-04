using NJsonSchema;
using Sentry.Core;
using System.Collections.Generic;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class VariantFieldDto : BaseFieldDto
    {
        public VariantFieldDto() : base() { }

        public VariantFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, int position, bool array) : base(prop, position, array) { }

        public VariantFieldDto(VariantField field) : base(field) { }

        public VariantFieldDto(SchemaRow row) : base(row) { }

        public override string FieldType => GlobalConstants.Datatypes.VARIANT;

        public override bool Nullable { get; set; }

        //Properties that are not utilized by this type and are defaulted
        public override int Precision { get; set; }
        public override int Scale { get; set; }
        public override string SourceFormat { get; set; }
        public override int OrdinalPosition { get; set; }

        public override bool CompareToEntity(BaseField field)
        {
            return SchemaExtensions.TryConvertTo<VariantField>(field) == null;
        }

        public override BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision)
        {
            BaseField newEntityField = new VariantField();
            ToEntity(newEntityField, parentField, parentRevision);
            return newEntityField;
        }

        public override void Clean(string extension)
        {
            //Nothing to clean
        }
    }
}
