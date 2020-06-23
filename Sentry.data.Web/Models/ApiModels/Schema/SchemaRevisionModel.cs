using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Schema
{
    public class SchemaRevisionModel
    {
        public int RevisionId { get; set; }
        public int RevisionNumber { get; set; }
        public string SchemaRevisionName { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public string CreatedDTM { get; set; }
        public string LastUpdatedDTM { get; set; }
        public string JsonSchemaObject { get; set; }
    }
}