using System;

namespace Sentry.data.Core
{ 
    public class Metadata
    {

        public Metadata()
        {

        }
        public virtual int DataAsset_ID { get; set; }


        public virtual string DataElement_NME { get; set; }


        public virtual string DataObject_NME { get; set; }
        public virtual string DataObject_DSC { get; set; }
        public virtual string DataObjectCode_DSC { get; set; }


        public virtual string DataObjectDetailType_VAL { get; set; }
        public virtual string DataObjectField_NME { get; set; }
        public virtual string DataObjectField_DSC { get; set; }


        public virtual string Display_IND { get; set; }
    }

}
