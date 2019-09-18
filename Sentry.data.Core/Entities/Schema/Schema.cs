using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public abstract class Schema
    {
        protected Schema() { }
        protected Schema(DatasetFileConfig config, IApplicationUser user)
        {
            Name = config.Name;
            CreatedDTM = DateTime.Now;
            LastUpdatedDTM = DateTime.Now;
            CreatedBy = user.AssociateId;
        }
        public virtual int SchemaId { get; set; }
        public virtual string SchemaEntity_NME { get; set; }
        public virtual string Name { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual DateTime CreatedDTM { get; set; }
        public virtual DateTime LastUpdatedDTM { get; set; }

        //public virtual IEnumerable<SchemaRevision> Revisions { get; set; }
    }
}
