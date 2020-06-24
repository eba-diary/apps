using NJsonSchema;
using Sentry.data.Core.DTO.Schema.Fields;
using System.Collections.Generic;

namespace Sentry.data.Core.Factories.Fields
{
    public class DecimalFieldDtoFactory : FieldDtoFactory
    {
        private KeyValuePair<string, JsonSchemaProperty> _property;
        private readonly bool _array;
        private readonly DecimalField _baseField;
        private readonly SchemaRow _row;
        private readonly bool IsProperty;
        private readonly bool IsField;
        private readonly bool IsRow;

        public DecimalFieldDtoFactory(KeyValuePair<string, JsonSchemaProperty> prop, bool array)
        {
            _property = prop;
            _array = array;
            IsProperty = true;
        }

        public DecimalFieldDtoFactory(DecimalField field)
        {
            _baseField = field;
            IsField = true;
        }

        public DecimalFieldDtoFactory(SchemaRow row)
        {
            _row = row;
            IsRow = true;
        }

        public override BaseFieldDto GetField()
        {
            if (IsField)
            {
                return new DecimalFieldDto(_baseField);
            }
            else if (IsProperty)
            {
                return new DecimalFieldDto(_property, _array);
            }
            else if (IsRow)
            {
                return new DecimalFieldDto(_row);
            }
            else
            {
                return null;
            }
        }
    }
}
