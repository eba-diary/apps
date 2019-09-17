namespace Sentry.data.Core
{
    public class DecimalField : BaseField, ISchemaField
    {
        public override SchemaDatatypes Type
        {
            get
            {
                return SchemaDatatypes.DECIMAL;
            }
            set => Type = SchemaDatatypes.DECIMAL;
        }
        
        public int Precision { get; set; }
        public int Scale { get; set; }
    }
}
