using NJsonSchema;
using Sentry.Core;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.DTO.Schema.Fields
{
    public class VarcharFieldDto : BaseFieldDto
    {
        public VarcharFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, int rowPosition, bool array) : base(prop, rowPosition, array)
        {
            if (prop.Value.IsArray)
            {
                Length = prop.FindArraySchema()?.MaxLength ?? GlobalConstants.Datatypes.Defaults.VARCHAR_LENGTH_DEFAULT;
            }
            else
            {
                Length = prop.Value.MaxLength?? GlobalConstants.Datatypes.Defaults.VARCHAR_LENGTH_DEFAULT;
            }
        }

        public VarcharFieldDto(VarcharField field) : base(field)
        {
            Length = field.FieldLength;
            OrdinalPosition = field.OrdinalPosition;
        }
        
        public VarcharFieldDto(SchemaRow row) : base(row)
        {
            ValidationResults results = new ValidationResults();
            if (string.IsNullOrWhiteSpace(row.Length))
            {
                Length = GlobalConstants.Datatypes.Defaults.VARCHAR_LENGTH_DEFAULT;
            }
            else if (!int.TryParse(row.Length, out int x))
            {
                results.Add(OrdinalPosition.ToString(), $"({Name}) VARCHAR Length must be non-negative integer value");
            }
            else
            {
                Length = int.Parse(row.Length);
            }

            if (!results.IsValid())
            {
                throw new ValidationException(results);
            }
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

        public override ValidationResults Validate(string extension)
        {
            ValidationResults results = base.Validate(extension);

            //Varchar Length
            if (Length < 1 || Length > 16000000) //true max is 16777216
            {
                results.Add(OrdinalPosition.ToString(), $"({Name}) VARCHAR length ({Length}) is required to be between 1 and 16000000");
            }

            return results;
        }

        public override BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision)
        {
            BaseField newEntityField = new VarcharField()
            {
                //Apply defaults if neccessary
                FieldLength = (this.Length == 0) ? GlobalConstants.Datatypes.Defaults.VARCHAR_LENGTH_DEFAULT : this.Length
            };
            base.ToEntity(newEntityField, parentField, parentRevision);
            return newEntityField;
        }
    }
}
