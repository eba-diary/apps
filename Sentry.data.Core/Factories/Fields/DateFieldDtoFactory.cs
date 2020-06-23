using NJsonSchema;
using Sentry.data.Core.DTO.Schema.Fields;
using System.Collections.Generic;

namespace Sentry.data.Core.Factories.Fields
{
    public class DateFieldDtoFactory : FieldDtoFactory
    {
        private KeyValuePair<string, JsonSchemaProperty> _property;
        private readonly bool _array;
        private readonly DateField _baseField;
        private readonly SchemaRow _row;
        private readonly bool IsProperty;
        private readonly bool IsField;
        private readonly bool IsRow;

        public DateFieldDtoFactory(KeyValuePair<string, JsonSchemaProperty> prop, bool array)
        {
            _property = prop;
            _array = array;
            IsProperty = true;
        }

        public DateFieldDtoFactory(DateField field)
        {
            _baseField = field;
            IsField = true;
        }

        public DateFieldDtoFactory(SchemaRow row)
        {
            _row = row;
            IsRow = true;
        }

        public override BaseFieldDto GetField()
        {
            if (IsField)
            {
                return new DateFieldDto(_baseField);
            }
            else if (IsProperty)
            {
                return new DateFieldDto(_property, _array);
            }
            else if (IsRow)
            {
                return new DateFieldDto(_row);
            }
            else
            {
                return null;
            }
        }
    }
}
