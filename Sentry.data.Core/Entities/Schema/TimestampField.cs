namespace Sentry.data.Core
{
    public class TimestampField : BaseField, ISchemaField
    {
        public override SchemaDatatypes FieldType
        {
            get
            {
                return SchemaDatatypes.TIMESTAMP;
            }
            set => FieldType = SchemaDatatypes.TIMESTAMP;
        }
        public virtual string SourceFormat { get; set; }
    }
}
