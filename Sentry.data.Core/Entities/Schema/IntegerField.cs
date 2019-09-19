namespace Sentry.data.Core
{
    public class IntegerField : BaseField, ISchemaField
    {
        public override SchemaDatatypes FieldType
        {
            get
            {
                return SchemaDatatypes.INTEGER;
            }
            set => FieldType = SchemaDatatypes.INTEGER;
        }
    }
}
