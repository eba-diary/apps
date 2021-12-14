using Newtonsoft.Json.Linq;

namespace Sentry.data.Core
{
    public class StructField : BaseField, ISchemaField
    {
        public override SchemaDatatypes FieldType
        {
            get
            {
                return SchemaDatatypes.STRUCT;
            }
            set => FieldType = SchemaDatatypes.STRUCT;
        }

        protected override JObject GetJsonTypeDefinition()
        {
            return new JObject()
            {
                { "type", "object" }
            };
        }
    }
}
