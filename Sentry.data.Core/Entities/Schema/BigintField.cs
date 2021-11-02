﻿namespace Sentry.data.Core
{
    public class BigIntField : BaseField, ISchemaField
    {
        public override SchemaDatatypes FieldType
        {
            get
            {
                return SchemaDatatypes.BIGINT;
            }
            set => FieldType = SchemaDatatypes.BIGINT;
        }
    }
}