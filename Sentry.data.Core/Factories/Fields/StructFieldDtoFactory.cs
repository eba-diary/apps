using NJsonSchema;
using Sentry.data.Core.DTO.Schema.Fields;
using System.Collections.Generic;

namespace Sentry.data.Core.Factories.Fields
{
    public class StructFieldDtoFactory : FieldDtoFactory
    {
        private KeyValuePair<string, JsonSchemaProperty> _property;
        private readonly bool _array;
        private readonly int _position;
        private readonly StructField _baseField;
        private readonly SchemaRow _row;
        private readonly bool IsProperty;
        private readonly bool IsField;
        private readonly bool IsRow;

        public StructFieldDtoFactory(KeyValuePair<string, JsonSchemaProperty> prop, int position, bool array)
        {
            _property = prop;
            _array = array;
            _position = position;
            IsProperty = true;
        }

        public StructFieldDtoFactory(StructField field)
        {
            _baseField = field;
            IsField = true;
        }

        public StructFieldDtoFactory(SchemaRow row)
        {
            _row = row;
            IsRow = true;
        }

        public override BaseFieldDto GetField()
        {
            if (IsField)
            {
                return new StructFieldDto(_baseField);
            }
            else if (IsProperty)
            {
                return new StructFieldDto(_property, _position, _array);
            }
            else if (IsRow)
            {
                return new StructFieldDto(_row);
            }
            else
            {
                return null;
            }
        }
    }
}
