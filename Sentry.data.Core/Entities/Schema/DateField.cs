namespace Sentry.data.Core
{
    public class DateField : BaseField, ISchemaField
    {
        public override SchemaDatatypes FieldType
        {
            get
            {
                return SchemaDatatypes.DATE;
            }
            set => FieldType = SchemaDatatypes.DATE;
        }
        
        public virtual string SourceFormat { get; set; }
    }
}
