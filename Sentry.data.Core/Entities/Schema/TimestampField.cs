namespace Sentry.data.Core
{
    public class TimestampField : BaseField, ISchemaField
    {
        public override SchemaDatatypes Type
        {
            get
            {
                return SchemaDatatypes.TIMESTAMP;
            }
            set => Type = SchemaDatatypes.TIMESTAMP;
        }
        public string SourceFormat { get; set; }
    }
}
