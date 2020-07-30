namespace Sentry.data.Core
{
    public class VarcharField : BaseField, ISchemaField
    {
        public override SchemaDatatypes FieldType
        {
            get
            {
                return SchemaDatatypes.VARCHAR;
            }
            set => FieldType = SchemaDatatypes.VARCHAR;
        }
    }
}
