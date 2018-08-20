using Sentry.data.Core.Entities.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Metadata
{
    public class Schema
    {
        public Schema()
        {

        }

        public virtual int Schema_ID { get; set; }
        public virtual int Revision_ID { get; set; }
        public virtual int DataObject_ID { get; set; }
        public virtual DatasetFileConfig DatasetFileConfig { get; set; }

        public virtual IList<DatasetFile> DatasetFiles { get; set; }
        public virtual IList<HiveTable> HiveTables { get; set; }

        public virtual string Schema_NME { get; set; }
        public virtual string Schema_DSC { get; set; }
        public virtual Boolean IsForceMatch { get; set; }

        public virtual DateTime Created_DTM { get; set; }
        public virtual DateTime Changed_DTM { get; set; }
    }
}
