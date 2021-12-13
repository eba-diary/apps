using Newtonsoft.Json.Linq;

namespace Sentry.data.Core
{
    public class SchemaRevisionJsonStructureDto
    {
        public SchemaRevisionDto Revision { get; set; }
        public JObject JsonStructure { get; set; }
    }
}
