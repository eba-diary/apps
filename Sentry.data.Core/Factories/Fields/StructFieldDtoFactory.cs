using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.DTO.Schema.Fields;

namespace Sentry.data.Core.Factories.Fields
{
    public class StructFieldDtoFactory : FieldDtoFactory
    {
        private KeyValuePair<string, JsonSchemaProperty> _property;
        private bool _array;
        private StructField _baseField;
        private SchemaRow _row;
        private readonly bool IsProperty;
        private readonly bool IsField;
        private readonly bool IsRow;

        public StructFieldDtoFactory(KeyValuePair<string, JsonSchemaProperty> prop, bool array)
        {
            _property = prop;
            _array = array;
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
                return new StructFieldDto(_property, _array);
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
