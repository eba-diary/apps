using Newtonsoft.Json.Linq;

namespace Sentry.data.Web.Models.ApiModels.Schema
{
    public class SchemaRevisionJsonStructureModel : SchemaRevisionModel
    {
        public JObject JsonStructure { get; set; }
    }
}