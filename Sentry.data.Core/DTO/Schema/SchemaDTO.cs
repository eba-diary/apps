using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public abstract class SchemaDto
    {
        public int SchemaId { get; set; }
        public abstract string SchemaEntity_NME { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ParentDatasetId { get; set; }
        public virtual bool DeleteInd { get; set; }
        public virtual string DeleteIssuer { get; set; }
        public virtual DateTime DeleteIssueDTM { get; set; }
    }
}
