using Newtonsoft.Json.Linq;

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
        }
        
        public virtual int Precision { get; set; }
        public virtual int Scale { get; set; }

        protected override JObject GetJsonTypeDefinition()
        {
            return new JObject()
            {
                { "type", "number"},
                { "dsc-precision", Precision },
                { "dsc-scale", Scale }
            };
        }
    }
}
