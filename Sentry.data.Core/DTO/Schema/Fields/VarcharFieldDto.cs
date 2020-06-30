using NJsonSchema;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class VarcharFieldDto : BaseFieldDto
    {
        public VarcharFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, bool array) : base(prop, array)
        {
            Length = (prop.Value.MaxLength) ?? 0;
        }

        public VarcharFieldDto(VarcharField field) : base(field)
        {
            Length = field.FieldLength;
            OrdinalPosition = field.OrdinalPosition;
        }

        public VarcharFieldDto(SchemaRow row) : base(row)
        {
#pragma warning disable S3240 // The simplest possible condition syntax should be used
            if (!String.IsNullOrWhiteSpace(row.Length))
#pragma warning restore S3240 // The simplest possible condition syntax should be used
            {
                Length = (Int32.TryParse(row.Length, out int x)) ? x : 0;
            }
            else
            {
                Length = 0;
            }
            OrdinalPosition = row.Position;
        }

        public override string FieldType => GlobalConstants.Datatypes.VARCHAR;
        public override bool Nullable { get; set; }
        public override int Length { get; set; }

        
        //Properties that are not utilized by this type and are defaulted
        public override int Precision { get; set; }
        public override int Scale { get; set; }
        public override string SourceFormat { get; set; }
        public override int OrdinalPosition { get; set; }

        public override bool CompareToEntity(BaseField field)
        {
            bool changed = false;
            if (SchemaExtensions.TryConvertTo<VarcharField>(field) == null)
            {
                changed = true;
            }
            return changed;
        }

        public override BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision)
        {
            BaseField newEntityField = new VarcharField()
            {
                //Apply defaults if neccessary
                FieldLength = (this.Length == 0) ? 8000 : this.Length
            };
            base.ToEntity(newEntityField, parentField, parentRevision);
            return newEntityField;
        }
    }
}
