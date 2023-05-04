using Newtonsoft.Json.Linq;

namespace Sentry.data.Core
{
    public class BigIntField : BaseField, ISchemaField
    {
        public override SchemaDatatypes FieldType
        {
            get
            {
                return SchemaDatatypes.BIGINT;
            }
        }

        protected override JObject GetJsonTypeDefinition()
        {
            return new JObject()
            {
                { "type", "integer"},
                { "format", "biginteger" }
            };
        }
    }
}
