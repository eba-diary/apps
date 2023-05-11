using Newtonsoft.Json.Linq;

namespace Sentry.data.Core
{
    public class IntegerField : BaseField, ISchemaField
    {
        public override SchemaDatatypes FieldType
        {
            get
            {
                return SchemaDatatypes.INTEGER;
            }
        }

        protected override JObject GetJsonTypeDefinition()
        {
            return new JObject()
            {
                { "type", "integer"}
            };
        }
    }
}
