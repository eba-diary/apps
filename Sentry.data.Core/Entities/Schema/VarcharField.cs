using Newtonsoft.Json.Linq;

namespace Sentry.data.Core
{
    public class VarcharField : BaseField, ISchemaField
    {
        public override SchemaDatatypes FieldType
        {
            get
            {
                return SchemaDatatypes.VARCHAR;
            }
        }

        protected override JObject GetJsonTypeDefinition()
        {
            return new JObject()
            {
                { "type", "string" },
                { "maxlength", FieldLength }
            };
        }
    }
}
