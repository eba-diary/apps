using NJsonSchema;
using Sentry.Core;
using System.Collections.Generic;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class StructFieldDto : BaseFieldDto
    {
        public StructFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, int position, bool array) : base(prop, position, array) { }

        public StructFieldDto(StructField field) : base(field) { }

        public StructFieldDto(SchemaRow row) : base(row) { }

        public override string FieldType => GlobalConstants.Datatypes.STRUCT;
        public override bool Nullable { get; set; }


        //Properties that are not utilized by this type and are defaulted
        public override int Precision { get; set; }
        public override int Scale { get; set; }
        public override string SourceFormat { get; set; }
        public override int OrdinalPosition { get; set; }

        public override bool CompareToEntity(BaseField field)
        {
            bool changed = false;
            if (SchemaExtensions.TryConvertTo<StructField>(field) == null)
            {
                changed = true;
            }
            return changed;
        }

        public override ValidationResults Validate(string extension)
        {
            ValidationResults results = base.Validate(extension);

            //Struct has children
            if (!HasChildren)
            {
                results.Add(OrdinalPosition.ToString(), $"({Name}) STRUCTs are required to have children");
            }

            return results;
        }

        public override BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision)
        {
            BaseField newEntityField = new StructField();
            base.ToEntity(newEntityField, parentField, parentRevision);
            return newEntityField;
        }

        public override void Clean(string extension)
        {
            //Nothing to clean
        }
    }
}
