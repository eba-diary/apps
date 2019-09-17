namespace Sentry.data.Core
{
    public class StructField : BaseField, ISchemaField
    {
        public override SchemaDatatypes Type
        {
            get
            {
                return SchemaDatatypes.STRUCT;
            }
            set => Type = SchemaDatatypes.STRUCT;
        }
    }
}
