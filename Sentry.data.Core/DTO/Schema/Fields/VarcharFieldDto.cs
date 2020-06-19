using NJsonSchema;
using System.Collections.Generic;
using System;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class VarcharFieldDto : BaseFieldDto
    {
        private int _scale;
        private int _precision;
        private string _sourceFormat;
        private int _ordinalPosition;

        public VarcharFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, bool array) : base(prop, array)
        {
            Length = (prop.Value.MaxLength) ?? 0;
        }

        public VarcharFieldDto(VarcharField field) : base(field)
        {
            Length = field.FieldLength;
        }

        public VarcharFieldDto(SchemaRow row) : base(row)
        {
            if (!String.IsNullOrWhiteSpace(row.Length))
            {
                Length = (Int32.TryParse(row.Length, out int x)) ? x : 0;
            }
            else
            {
                Length = 0;
            }
        }

        public override string FieldType => GlobalConstants.Datatypes.VARCHAR;
        public override bool Nullable { get; set; }
        public override int Length { get; set; }

        
        //Properties that are not utilized by this type and are defaulted
        public override int Precision { get { return _precision; } set { _precision = 0; } }
        public override int Scale { get { return _scale; } set { _scale = 0; } }
        public override string SourceFormat { get { return _sourceFormat; } set { _sourceFormat = null; } }
        public override int OrdinalPosition { get { return _ordinalPosition; } set { _ordinalPosition = 0; } }

        public override bool CompareToEntity(BaseField field)
        {
            bool changed = false;
            if (SchemaExtensions.TryConvertTo<VarcharField>(field) == null) { changed = true; }
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
