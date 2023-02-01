using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SchemaRevisionDto
    {
        public int RevisionId { get; set; }
        public int RevisionNumber { get; set; }
        public string SchemaRevisionName { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDTM { get; set; }
        public DateTime LastUpdatedDTM { get; set; }
        public string JsonSchemaObject { get; set; }
        public int SchemaId { get; set; }
    }
}
