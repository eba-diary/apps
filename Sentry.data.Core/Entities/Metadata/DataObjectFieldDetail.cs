using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Metadata
{
    public class DataObjectFieldDetail
    {
        public DataObjectFieldDetail()
        {

        }

        public virtual DataObjectField DataObjectField { get; set; }

        public virtual int DataObjectFieldDetail_ID { get; set; }
        public virtual int DataObjectField_ID { get; set; }
        public virtual int DataTag_ID { get; set; }
        public virtual DateTime DataObjectFieldDetailCreate_DTM { get; set; }
        public virtual DateTime DataObjectFieldDetailChange_DTM { get; set; }
        public virtual string DataObjectFieldDetailType_CDE { get; set; }
        public virtual string DataObjectFieldDetailType_VAL { get; set; }
        public virtual DateTime LastUpdt_DTM { get; set; }
        public virtual string BusFieldKey { get; set; }
    }
}
