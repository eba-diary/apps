using System.Collections.Generic;

namespace Sentry.data.Web.Models.ApiModels.Schema
{
    public class SchemaRevisionDetailModel
    {
        public SchemaRevisionModel Revision { get; set; }
        public List<SchemaFieldModel> Fields { get; set; } = new List<SchemaFieldModel>();
    }
}