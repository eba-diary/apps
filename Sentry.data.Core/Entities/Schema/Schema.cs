using System;
using System.Collections.Generic;
using System.Linq;

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
        public virtual string UpdatedBy { get; set; }

        public virtual IList<SchemaRevision> Revisions { get; set; }

        protected internal virtual void AddRevision(SchemaRevision revision)
        {
            revision.Revision_NBR = (Revisions.Any()) ? Revisions.Count + 1 : 1;
            revision.ParentSchema = this;
            Revisions.Add(revision);
        }
    }
}
