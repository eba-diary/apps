namespace Sentry.data.Core
{
    public class DateField : BaseField, ISchemaField
    {
        public override SchemaDatatypes Type
        {
            get
            {
                return SchemaDatatypes.DATE;
            }
            set => Type = SchemaDatatypes.DATE;
        }
        
        public string SourceFormat { get; set; }
    }
}
