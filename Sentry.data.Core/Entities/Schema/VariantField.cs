using Newtonsoft.Json.Linq;
using System.Linq;

namespace Sentry.data.Core
{
    public class VariantField : BaseField, ISchemaField
    {
        public override SchemaDatatypes FieldType
        {
            get
            {
                return SchemaDatatypes.VARIANT;
            }
        }

        protected override JObject GetJsonTypeDefinition()
        {
            JObject definition = new JObject() { { "type", "variant" } };

            return definition;
        }
    }
}
