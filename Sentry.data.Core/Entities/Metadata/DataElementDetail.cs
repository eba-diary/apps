using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataElementDetail
    {
        public DataElementDetail()
        {

        }

        public virtual int DataElementDetail_ID { get; set; }
        //public virtual DataElement DataElement { get; set; }
        public virtual DateTime DataElementDetailCreate_DTM { get; set; }
        public virtual DateTime DataElementDetailChange_DTM { get; set; }
        public virtual string DataElementDetailType_CDE { get; set; }
        public virtual string DataElementDetailType_VAL { get; set; }
        public virtual DateTime LastUpdt_DTM { get; set; }
        public virtual string BusElementKey { get; set; }
    }
}
