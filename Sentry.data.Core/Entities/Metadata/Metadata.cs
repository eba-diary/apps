using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{ 
    public class Metadata
    {

        public Metadata()
        {

        }

        public virtual int DataAsset_ID { get; set; }
        public virtual String DataElement_NME { get; set; }
        public virtual String DataObject_NME { get; set; }
        public virtual String DataObjectCode_DSC { get; set; }
        public virtual String DataObjectDetailType_VAL { get; set; }
        public virtual String DataObjectField_NME { get; set; }
        public virtual int Display_IND { get; set; }
    }

}
