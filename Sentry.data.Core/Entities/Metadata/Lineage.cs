using Sentry.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class Lineage : Metadata
    {
        public Lineage()
        {

        }

        public virtual int ID { get; set; }

        public virtual String SourceElement_NME { get; set; }
        public virtual String SourceObject_NME { get; set; }
        public virtual String SourceObjectField_NME { get; set; }
        public virtual String Source_TXT { get; set; }
        public virtual String Transformation_TXT { get; set; }
        public virtual String DataObjectField_DSC { get; set; }
        public virtual String DataObject_DSC { get; set; }
    }
}
