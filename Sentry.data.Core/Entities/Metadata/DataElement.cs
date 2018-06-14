using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Metadata
{
    public class DataElement
    {
        public DataElement()
        {

        }

        public virtual IList<DataElementDetail> DataElementDetails { get; set; }

        public virtual IList<DataObject> DataObjects { get; set; }

        public virtual MetadataAsset MetadataAsset { get; set; }

        public virtual int DataElement_ID { get; set; }
        public virtual int DataTag_ID { get; set; }
        public virtual string DataElement_NME { get; set; }
        public virtual string DataElement_DSC { get; set; }
        public virtual string DataElement_CDE { get; set; }
        public virtual string DataElementCode_DSC { get; set; }
        public virtual DateTime DataElementCreate_DTM { get; set; }
        public virtual DateTime DataElementChange_DTM { get; set; }
        public virtual DateTime LastUpdt_DTM { get; set; }
        public virtual int DataAsset_ID{ get; set; }
        public virtual string BusElementKey { get; set; }
    }

    public class DataElementCode
    {
        public const String BusinessTerm = "Business Term";
        public const String Lineage = "Lineage";
        public const String DataFile = "Data File";
    }
}
