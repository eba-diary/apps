using Newtonsoft.Json.Linq;
using System.Linq;

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

        public override void SetStructurePosition(int index)
        {
            if (ParentField == null)
            {
                StructurePosition = index.ToString();
            }
            else
            {
                StructurePosition = $"{ParentField.StructurePosition}.{index}";
            }
        }

        protected override JObject GetJsonTypeDefinition()
        {
            JObject definition = new JObject() { { "type", "object" } };

            if (ChildFields?.Any() == true)
            {
                definition.AddJsonStructureProperties(ChildFields);
            }

            return definition;
        }
    }
}
