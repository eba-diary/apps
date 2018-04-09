using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Metadata
{
    public class DataObjectDetail
    {
        public DataObjectDetail()
        { }

        public virtual int DataObjectDetail_ID { get; set; }

        public virtual DataObject DataObject { get; set; }

        public virtual int DataObject_ID { get; set; }
        public virtual DateTime DataObjectDetailCreate_DTM { get; set; }
        public virtual DateTime DataObjectDetailChange_DTM { get; set; }
        public virtual string DataObjectDetailType_CDE { get; set; }
        public virtual string DataObjectDetailType_VAL { get; set; }
        public virtual DateTime LastUpdt_DTM { get; set; }
        public virtual string BusObjectKey { get; set; }
    }
}
