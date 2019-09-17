using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class FileSchema
    {
        public virtual Guid SchemaId { get; set; }
        public virtual string SchemaEntity_NME { get; set; }
        public virtual StructField SchemaStruct { get; set; }
        public virtual string Name { get; set; }
        public virtual FileExtension Extension { get; set; }
        public virtual int RevisionId { get; set; }
        public virtual string RevisionName { get; set; }
        public virtual IEnumerable<BaseField> Fields { get; set; }
    }
}
