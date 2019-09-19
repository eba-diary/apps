namespace Sentry.data.Core
{
    public class DecimalField : BaseField, ISchemaField
    {
        public override SchemaDatatypes FieldType
        {
            get
            {
                return SchemaDatatypes.DECIMAL;
            }
            set => FieldType = SchemaDatatypes.DECIMAL;
        }
        
        public virtual int Precision { get; set; }
        public virtual int Scale { get; set; }
    }
}
