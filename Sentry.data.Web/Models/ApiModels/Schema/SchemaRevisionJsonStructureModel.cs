using Newtonsoft.Json.Linq;

namespace Sentry.data.Web.Models.ApiModels.Schema
{
    public class SchemaRevisionJsonStructureModel
    {
        public SchemaRevisionModel Revision { get; set; }
        public JObject JsonStructure { get; set; }
    }
}