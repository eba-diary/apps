using Newtonsoft.Json.Linq;

namespace Sentry.data.Core
{
    public class SchemaRevisionJsonStructureDto : SchemaRevisionDto
    {
        public JObject JsonStructure { get; set; }
    }
}
