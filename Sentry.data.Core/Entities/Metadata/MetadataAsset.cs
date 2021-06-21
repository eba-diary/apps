using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class MetadataAsset
    {
        public MetadataAsset()
        {

        }

        public virtual int DataAsset_ID { get; set; }
        public virtual string DataAsset_NME { get; set; }
        public virtual string DataAsset_DSC { get; set; }
        public virtual string DataAssetOwner_NME { get; set; }
        public virtual DateTime LastUpdt_DTM { get; set; }

        //public virtual IList<DataElement> DataElements { get; set; }
    }
}
