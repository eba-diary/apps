using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class SchemaRevision
    {
        public virtual int SchemaRevision_Id { get; set; }
        public virtual string SchemaRevision_Name { get; set; }
        public virtual int SchemaStruct_Id { get; set; }
        public virtual IEnumerable<BaseField> Fields { get; set; }
    }
}
