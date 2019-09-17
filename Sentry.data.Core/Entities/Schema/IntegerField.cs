namespace Sentry.data.Core
{
    public class IntegerField : BaseField, ISchemaField
    {
        public override SchemaDatatypes Type
        {
            get
            {
                return SchemaDatatypes.INTEGER;
            }
            set => Type = SchemaDatatypes.INTEGER;
        }
    }
}
