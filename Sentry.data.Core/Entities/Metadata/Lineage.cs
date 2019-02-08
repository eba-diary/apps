using System;
using System.Collections.Generic;

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

        public LineageCreation()
        {
        }

        public LineageCreation(Lineage l, int layer)
        {
            DataAsset_ID = l.DataAsset_ID;
            Model_NME = l.Model_NME;
            Layer = layer;

            DataElement_NME = l.DataElement_NME;
            DataElement_TYP = l.DataElement_TYP;

            DataObject_NME = l.DataObject_NME;
            DataObject_DSC = l.DataObject_DSC;
            DataObjectCode_DSC = l.DataObjectCode_DSC;

            DataObjectDetailType_VAL = l.DataObjectDetailType_VAL;
            DataObjectField_NME = l.DataObjectField_NME;
            DataObjectField_DSC = l.DataObjectField_DSC;

            SourceElement_NME = l.SourceElement_NME;
            SourceField_NME = l.SourceField_NME;
            SourceObject_NME = l.SourceObject_NME;

            Display_IND = l.Display_IND;
            Sources = new List<LineageCreation>();

            DataLineage_ID = l.DataLineage_ID;
            Transformation_TXT = l.Transformation_TXT;
            Source_TXT = l.Source_TXT;
        }




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
