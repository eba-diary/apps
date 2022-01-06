using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public static class JsonStructureExtensions
    {
        public static void AddJsonStructureProperties(this JObject structure, IEnumerable<BaseField> fields)
        {
            JObject properties = new JObject();

            foreach (BaseField field in fields.OrderBy(x => x.OrdinalPosition).ToList())
            {
                properties.Add(field.Name, field.ToJsonPropertyDefinition());
            }

            structure.Add("properties", properties);
        }
    }
}
