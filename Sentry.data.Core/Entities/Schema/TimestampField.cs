using Newtonsoft.Json.Linq;

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
        }
        public virtual string SourceFormat { get; set; }

        protected override JObject GetJsonTypeDefinition()
        {
            return new JObject()
            {
                { "type", "string"},
                { "format", "date-time"},
                { "dsc-format", SourceFormat}
            };
        }
    }
}
