using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Metadata
{
    public class DataObjectField
    {
        public DataObjectField()
        {

        }

        public virtual IList<DataObjectFieldDetail> DataObjectFieldDetails { get; set; }

        public virtual int DataObjectField_ID { get; set; }
        public virtual int DataObject_ID { get; set; }
        public virtual DataObject DataObject { get; set; }

        public virtual int DataTag_ID { get; set; }
        public virtual string DataObjectField_NME { get; set; }
        public virtual string DataObjectField_DSC { get; set; }
        public virtual DateTime DataObjectFieldCreate_DTM { get; set; }
        public virtual DateTime DataObjectFieldChange_DTM { get; set; }
        public virtual DateTime LastUpdt_DTM { get; set; }
        public virtual string BusObjectKey { get; set; }
        public virtual string BusFieldKey { get; set; }
    }
}
