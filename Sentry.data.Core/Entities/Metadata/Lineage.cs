using Sentry.Core;
using Sentry.data.Core.Entities.Metadata;
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

        public virtual Guid DataLineage_ID { get; set; }


        public virtual String SourceElement_NME { get; set; }
        public virtual String SourceObject_NME { get; set; }
        public virtual String SourceField_NME { get; set; }
        public virtual String Transformation_TXT { get; set; }

        public virtual String Line_CDE { get; set; }
        public virtual String Model_NME { get; set; }
        public virtual String DataElement_TYP { get; set; }
        public virtual String Source_TXT { get; set; }
        public virtual String BusTerm_DSC { get; set; }
    }

    public class LineageCreation
    {

        public virtual int Layer { get; set; }

        public virtual Guid DataLineage_ID { get; set; }

        public virtual int DataAsset_ID { get; set; }

        public virtual String DataElement_NME { get; set; }

        public virtual String DataObject_NME { get; set; }
        public virtual String DataObject_DSC { get; set; }
        public virtual String DataObjectCode_DSC { get; set; }


        public virtual String DataObjectDetailType_VAL { get; set; }
        public virtual String DataObjectField_NME { get; set; }
        public virtual String DataObjectField_DSC { get; set; }

        public virtual String Display_IND { get; set; }

        public virtual List<LineageCreation> Sources { get; set; }


        public virtual String Transformation_TXT { get; set; }

        public virtual String SourceElement_NME { get; set; }
        public virtual String SourceObject_NME { get; set; }
        public virtual String SourceField_NME { get; set; }

        public virtual String Line_CDE { get; set; }
        public virtual String Model_NME { get; set; }
        public virtual String DataElement_TYP { get; set; }
        public virtual String Source_TXT { get; set; }
    }
}
