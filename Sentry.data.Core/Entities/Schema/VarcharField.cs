namespace Sentry.data.Core
{
    public class VarcharField : BaseField, ISchemaField
    {
        public override SchemaDatatypes Type
        {
            get
            {
                return SchemaDatatypes.VARCHAR;
            }
            set => Type = SchemaDatatypes.VARCHAR;
        }

        public int FieldLength { get; set; }
    }
}
