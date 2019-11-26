using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Schema
{
    public class SchemaRevisionDetailModel
    {
        public SchemaRevisionModel Revision { get; set; }
        public List<SchemaFieldModel> Fields { get; set; }
    }
}