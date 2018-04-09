using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Metadata
{
    public class DataObject
    {
        public DataObject()
        {

        }

        public virtual IList<DataObjectDetail> DataObjectDetails { get; set; }

        public virtual IList<DataObjectField> DataObjectFields { get; set; }

        public virtual int DataObject_ID { get; set; }

        public virtual DataElement DataElement { get; set; }

        public virtual int DataElement_ID { get; set; }
        public virtual int DataTag_ID { get; set; }
        public virtual int Reviewer_ID { get; set; }
        public virtual string DataObject_NME { get; set; }
        public virtual string DataObject_DSC { get; set; }
        public virtual int DataObjectParent_ID { get; set; }
        public virtual string DataObject_CDE { get; set; }
        public virtual string DataObjectCode_DSC { get; set; }
        public virtual DateTime DataObjectCreate_DTM { get; set; }
        public virtual DateTime DataObjectChange_DTM { get; set; }
        public virtual DateTime LastUpdt_DTM { get; set; }
        public virtual string BusElementKey { get; set; }
        public virtual string BusObjectKey { get; set; }
    }
}
