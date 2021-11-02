﻿using NJsonSchema;
using Sentry.data.Core.DTO.Schema.Fields;
using System.Collections.Generic;

namespace Sentry.data.Core.Factories.Fields
{
    public class IntegerFieldDtoFactory : FieldDtoFactory
    {
        private KeyValuePair<string, JsonSchemaProperty> _property;
        private readonly bool _array;
        private readonly int _rowPosition;
        private readonly IntegerField _baseField;
        private readonly SchemaRow _row;
        private readonly bool IsProperty;
        private readonly bool IsField;
        private readonly bool IsRow;

        public IntegerFieldDtoFactory(KeyValuePair<string, JsonSchemaProperty> prop, int rowPosition, bool array)
        {
            _property = prop;
            _array = array;
            _rowPosition = rowPosition;
            IsProperty = true;
        }

        public IntegerFieldDtoFactory(IntegerField field)
        {
            _baseField = field;
            IsField = true;
        }

        public IntegerFieldDtoFactory(SchemaRow row)
        {
            _row = row;
            IsRow = true;
        }

        public override BaseFieldDto GetField()
        {
            if (IsField)
            {
                return new IntegerFieldDto(_baseField);
            }
            else if (IsProperty)
            {
                return new IntegerFieldDto(_property, _rowPosition, _array);
            }
            else if (IsRow)
            {
                return new IntegerFieldDto(_row);
            }
            else
            {
                return null;
            }
        }
    }
}