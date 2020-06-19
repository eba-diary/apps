using NJsonSchema;
using Sentry.data.Core.DTO.Schema.Fields;
using System.Collections.Generic;

namespace Sentry.data.Core.Factories.Fields
{
    public class VarcharFieldDtoFactory : FieldDtoFactory
    {
        private KeyValuePair<string, JsonSchemaProperty> _property;
        private bool _array;
        private VarcharField _baseField;
        private SchemaRow _row;
        private readonly bool IsProperty;
        private readonly bool IsField;
        private readonly bool IsRow;

        public VarcharFieldDtoFactory(KeyValuePair<string, JsonSchemaProperty> prop, bool array)
        {
            _property = prop;
            _array = array;
            IsProperty = true;
        }

        public VarcharFieldDtoFactory(VarcharField field)
        {
            _baseField = field;
            IsField = true;
        }

        public VarcharFieldDtoFactory(SchemaRow row)
        {
            _row = row;
            IsRow = true;
        }

        public override BaseFieldDto GetField()
        {
            if (IsField)
            {
                return new VarcharFieldDto(_baseField);
            }
            else if (IsProperty)
            {
                return new VarcharFieldDto(_property, _array);
            }
            else if (IsRow)
            {
                return new VarcharFieldDto(_row);
            }
            else
            {
                return null;
            }
        }
    }
}
