using Newtonsoft.Json.Linq;

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
        }
        
        public virtual string SourceFormat { get; set; }

        protected override JObject GetJsonTypeDefinition()
        {
            return new JObject()
            {
                { "type", "string"},
                { "format", "date"},
                { "dsc-format", SourceFormat}
            };
        }
    }
}
