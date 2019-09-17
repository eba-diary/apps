namespace Sentry.data.Core.Entities.Schema
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
