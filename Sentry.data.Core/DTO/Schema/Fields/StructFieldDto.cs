using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class StructFieldDto : BaseFieldDto
    {
        private int _scale;
        private int _precision;
        private string _sourceFormat;
        private int _ordinalPosition;

        public StructFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, bool array) : base(prop, array) { }

        public StructFieldDto(StructField field) : base(field) { }

        public StructFieldDto(SchemaRow row) : base(row) { }

        public override string FieldType => GlobalConstants.Datatypes.STRUCT;
        public override bool Nullable { get; set; }
        public override int Length { get; set; }


        //Properties that are not utilized by this type and are defaulted
        public override int Precision { get => _precision; set => _precision = value; }
        public override int Scale { get => _scale; set => _scale = value; }
        public override string SourceFormat { get => _sourceFormat; set => _sourceFormat = value; }
        public override int OrdinalPosition { get => _ordinalPosition; set => _ordinalPosition = value; }

        public override bool CompareToEntity(BaseField field)
        {
            bool changed = false;
            if (SchemaExtensions.TryConvertTo<StructField>(field) == null)
            {
                changed = true;
            }
            return changed;
        }

        public override BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision)
        {
            BaseField newEntityField = new StructField();
            base.ToEntity(newEntityField, parentField, parentRevision);
            return newEntityField;
        }
    }
}
